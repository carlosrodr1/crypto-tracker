using DataCollector.Shared.Data;
using DataCollector.Shared.Interfaces;
using DataCollector.Shared.Repositories;
using DataCollector.Worker;
using DataCollector.Worker.HealthChecks;
using DataCollector.Worker.Services;
using DataCollector.Worker.Workers;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=/data/crypto.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddHttpClient("scraper", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "DataCollector-Worker/1.0");
})
.AddPolicyHandler((services, _) => HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: attempt =>
            TimeSpan.FromSeconds(Math.Pow(2, attempt)) +
            TimeSpan.FromMilliseconds(Random.Shared.Next(0, 500)),
        onRetry: (outcome, timespan, attempt, _) =>
        {
            var logger = services.GetRequiredService<ILogger<CryptoPriceService>>();
            logger.LogWarning(
                "HTTP retry {Attempt}/3 after {Wait:F1}s. Reason: {Reason}",
                attempt, timespan.TotalSeconds,
                outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
        }))
.AddPolicyHandler(HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 5,
        durationOfBreak: TimeSpan.FromSeconds(30)));

builder.Services.AddSingleton<ScrapeTrigger>();
builder.Services.AddSingleton<ScrapeMetrics>();
builder.Services.AddScoped<ICryptoPriceRepository, CryptoPriceRepository>();
builder.Services.AddScoped<IScraperService, CryptoPriceService>();
builder.Services.AddHostedService<ScraperWorker>();

builder.Services.AddHealthChecks()
    .AddCheck<ScraperHealthCheck>("scraper");

var app = builder.Build();

app.MapPost("/trigger", (ScrapeTrigger trigger) =>
{
    trigger.Request();
    return Results.Ok(new { message = "Scrape triggered. Check the worker logs." });
});

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var payload = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                durationMs = e.Value.Duration.TotalMilliseconds
            }),
            totalDurationMs = report.TotalDuration.TotalMilliseconds
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
});

app.Run();
