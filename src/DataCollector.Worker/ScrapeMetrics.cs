namespace DataCollector.Worker;

/// <summary>
/// Thread-safe singleton that tracks scraper execution metrics,
/// consumed by the health check endpoint.
/// </summary>
public sealed class ScrapeMetrics
{
    private long _lastRunTicks;
    private int _totalSaved;

    public DateTimeOffset? LastRunAt => _lastRunTicks == 0
        ? null
        : new DateTimeOffset(_lastRunTicks, TimeSpan.Zero);

    public int TotalSaved => _totalSaved;

    public void RecordRun(int newQuotesSaved)
    {
        Interlocked.Exchange(ref _lastRunTicks, DateTimeOffset.UtcNow.UtcTicks);
        Interlocked.Add(ref _totalSaved, newQuotesSaved);
    }
}
