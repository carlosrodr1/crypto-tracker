using DataCollector.Shared.Entities;

namespace DataCollector.Worker.Services;

public interface IScraperService
{
    Task<IReadOnlyList<CryptoPrice>> ScrapeAsync(CancellationToken cancellationToken = default);
}
