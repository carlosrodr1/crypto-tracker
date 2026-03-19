using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DataCollector.Worker.HealthChecks;

public sealed class ScraperHealthCheck : IHealthCheck
{
    private readonly ScrapeMetrics _metrics;
    private readonly IConfiguration _configuration;

    public ScraperHealthCheck(ScrapeMetrics metrics, IConfiguration configuration)
    {
        _metrics = metrics;
        _configuration = configuration;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (_metrics.LastRunAt is null)
            return Task.FromResult(HealthCheckResult.Healthy("Scraper is starting up — no runs yet."));

        var intervalMinutes = _configuration.GetValue<int>("Scraper:IntervalMinutes", 5);
        var elapsed = DateTimeOffset.UtcNow - _metrics.LastRunAt.Value;
        var staleThreshold = TimeSpan.FromMinutes(intervalMinutes * 3);

        if (elapsed > staleThreshold)
            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"Scraper appears stuck. Last run was {elapsed.TotalMinutes:F0} min ago (threshold: {staleThreshold.TotalMinutes:F0} min)."));

        return Task.FromResult(HealthCheckResult.Healthy(
            $"Last run: {_metrics.LastRunAt:u} ({elapsed.TotalSeconds:F0}s ago). Records saved this session: {_metrics.TotalSaved}."));
    }
}
