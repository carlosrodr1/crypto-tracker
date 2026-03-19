using DataCollector.Shared.Data;
using DataCollector.Shared.Interfaces;
using DataCollector.Worker.Services;
using Microsoft.EntityFrameworkCore;

namespace DataCollector.Worker.Workers;

public class ScraperWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ScraperWorker> _logger;
    private readonly ScrapeTrigger _trigger;
    private readonly ScrapeMetrics _metrics;
    private readonly TimeSpan _interval;

    public ScraperWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<ScraperWorker> logger,
        ScrapeTrigger trigger,
        ScrapeMetrics metrics,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _trigger = trigger;
        _metrics = metrics;
        var intervalMinutes = configuration.GetValue<int>("Scraper:IntervalMinutes", 5);
        _interval = TimeSpan.FromMinutes(intervalMinutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ScraperWorker started. Interval: {Interval}.", _interval);

        await InitializeDatabaseAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunScrapeAsync(stoppingToken);

            _logger.LogInformation("Next automatic run in {Interval}. POST /trigger to run now.", _interval);

            await WaitForNextRunAsync(stoppingToken);
        }
    }

    private async Task WaitForNextRunAsync(CancellationToken stoppingToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        cts.CancelAfter(_interval);

        try
        {
            await _trigger.Reader.ReadAsync(cts.Token);
            _logger.LogInformation("Manual scrape triggered via POST /trigger.");
        }
        catch (OperationCanceledException)
        {
            // Interval elapsed or service stopping — both are expected
        }
    }

    private async Task InitializeDatabaseAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync(cancellationToken);
        await db.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;", cancellationToken);
        _logger.LogInformation("Database initialized with WAL mode.");
    }

    private async Task RunScrapeAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting scrape run at {Time}.", DateTimeOffset.UtcNow);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var scraperService = scope.ServiceProvider.GetRequiredService<IScraperService>();
            var repository = scope.ServiceProvider.GetRequiredService<ICryptoPriceRepository>();

            var prices = await scraperService.ScrapeAsync(cancellationToken);

            if (prices.Count > 0)
                await repository.AddRangeAsync(prices);

            _metrics.RecordRun(prices.Count);
            _logger.LogInformation(
                "Scrape run complete. {Count} prices saved. Session total: {SessionTotal}.",
                prices.Count, _metrics.TotalSaved);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error during scrape run.");
        }
    }
}
