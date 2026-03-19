using DataCollector.Shared.Data;
using DataCollector.Shared.Entities;
using DataCollector.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataCollector.Shared.Repositories;

public class CryptoPriceRepository : ICryptoPriceRepository
{
    private readonly AppDbContext _context;

    public CryptoPriceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CryptoPrice>> GetLatestAsync(int page, int pageSize)
    {
        var latestTime = await _context.CryptoPrices.MaxAsync(p => (DateTime?)p.CollectedAt);
        if (latestTime is null) return Enumerable.Empty<CryptoPrice>();

        return await _context.CryptoPrices
            .Where(p => p.CollectedAt == latestTime.Value)
            .OrderByDescending(p => (double)p.MarketCap)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<CryptoPrice?> GetByIdAsync(int id)
        => await _context.CryptoPrices.FindAsync(id);

    public async Task<IEnumerable<CryptoPrice>> GetHistoryBySymbolAsync(string symbol)
    {
        return await _context.CryptoPrices
            .Where(p => p.Symbol.ToLower() == symbol.ToLower())
            .OrderByDescending(p => p.CollectedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<CryptoPrice>> GetTopGainersAsync(int limit = 10)
    {
        var latestTime = await _context.CryptoPrices.MaxAsync(p => (DateTime?)p.CollectedAt);
        if (latestTime is null) return Enumerable.Empty<CryptoPrice>();

        return await _context.CryptoPrices
            .Where(p => p.CollectedAt == latestTime.Value)
            .OrderByDescending(p => (double)p.Change24h)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<CryptoPrice>> GetTopLosersAsync(int limit = 10)
    {
        var latestTime = await _context.CryptoPrices.MaxAsync(p => (DateTime?)p.CollectedAt);
        if (latestTime is null) return Enumerable.Empty<CryptoPrice>();

        return await _context.CryptoPrices
            .Where(p => p.CollectedAt == latestTime.Value)
            .OrderBy(p => (double)p.Change24h)
            .Take(limit)
            .ToListAsync();
    }

    public async Task AddRangeAsync(IEnumerable<CryptoPrice> prices)
    {
        await _context.CryptoPrices.AddRangeAsync(prices);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetTotalCountAsync()
        => await _context.CryptoPrices.CountAsync();

    public async Task<DateTime?> GetLastCollectedAtAsync()
        => await _context.CryptoPrices.MaxAsync(p => (DateTime?)p.CollectedAt);
}
