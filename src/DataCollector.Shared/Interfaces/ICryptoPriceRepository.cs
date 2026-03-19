using DataCollector.Shared.Entities;

namespace DataCollector.Shared.Interfaces;

public interface ICryptoPriceRepository
{
    Task<IEnumerable<CryptoPrice>> GetLatestAsync(int page, int pageSize);
    Task<CryptoPrice?> GetByIdAsync(int id);
    Task<IEnumerable<CryptoPrice>> GetHistoryBySymbolAsync(string symbol);
    Task<IEnumerable<CryptoPrice>> GetTopGainersAsync(int limit = 10);
    Task<IEnumerable<CryptoPrice>> GetTopLosersAsync(int limit = 10);
    Task AddRangeAsync(IEnumerable<CryptoPrice> prices);
    Task<int> GetTotalCountAsync();
    Task<DateTime?> GetLastCollectedAtAsync();
}
