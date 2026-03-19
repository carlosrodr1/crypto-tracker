using DataCollector.Shared.Data;
using DataCollector.Shared.Entities;
using DataCollector.Shared.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DataCollector.Tests;

public class CryptoPriceRepositoryTests
{
    private static AppDbContext CreateDb(string name)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        return new AppDbContext(options);
    }

    private static CryptoPrice MakePrice(string coinId, string symbol, decimal change24h, DateTime? collectedAt = null) =>
        new()
        {
            CoinId = coinId,
            Symbol = symbol,
            Name = symbol,
            PriceUsd = 100m,
            Change24h = change24h,
            MarketCap = 1_000_000,
            Volume24h = 500_000,
            CollectedAt = collectedAt ?? DateTime.UtcNow
        };

    [Fact]
    public async Task AddRangeAsync_PersistsAllRecords()
    {
        using var db = CreateDb(nameof(AddRangeAsync_PersistsAllRecords));
        var repo = new CryptoPriceRepository(db);

        await repo.AddRangeAsync(new[]
        {
            MakePrice("bitcoin", "BTC", 2.5m),
            MakePrice("ethereum", "ETH", -1.2m)
        });

        Assert.Equal(2, await db.CryptoPrices.CountAsync());
    }

    [Fact]
    public async Task GetLatestAsync_ReturnsOnlyMostRecentBatch()
    {
        using var db = CreateDb(nameof(GetLatestAsync_ReturnsOnlyMostRecentBatch));
        var repo = new CryptoPriceRepository(db);

        var old = DateTime.UtcNow.AddMinutes(-10);
        var recent = DateTime.UtcNow;

        await repo.AddRangeAsync(new[]
        {
            MakePrice("bitcoin", "BTC", 1m, old),
            MakePrice("bitcoin", "BTC", 3m, recent),
            MakePrice("ethereum", "ETH", 2m, recent)
        });

        var latest = (await repo.GetLatestAsync(1, 10)).ToList();

        Assert.Equal(2, latest.Count);
        Assert.All(latest, p => Assert.Equal(recent, p.CollectedAt));
    }

    [Fact]
    public async Task GetLatestAsync_ReturnsPaginatedResults()
    {
        using var db = CreateDb(nameof(GetLatestAsync_ReturnsPaginatedResults));
        var repo = new CryptoPriceRepository(db);

        var at = DateTime.UtcNow;
        await repo.AddRangeAsync(Enumerable.Range(1, 5)
            .Select(i => MakePrice($"coin{i}", $"C{i}", i, at)));

        var page1 = (await repo.GetLatestAsync(1, 3)).ToList();
        var page2 = (await repo.GetLatestAsync(2, 3)).ToList();

        Assert.Equal(3, page1.Count);
        Assert.Equal(2, page2.Count);
    }

    [Fact]
    public async Task GetTopGainersAsync_ReturnsSortedByChange24hDescending()
    {
        using var db = CreateDb(nameof(GetTopGainersAsync_ReturnsSortedByChange24hDescending));
        var repo = new CryptoPriceRepository(db);

        var at = DateTime.UtcNow;
        await repo.AddRangeAsync(new[]
        {
            MakePrice("a", "A", 5m, at),
            MakePrice("b", "B", -3m, at),
            MakePrice("c", "C", 12m, at)
        });

        var gainers = (await repo.GetTopGainersAsync(2)).ToList();

        Assert.Equal(2, gainers.Count);
        Assert.Equal("C", gainers[0].Symbol);
        Assert.Equal("A", gainers[1].Symbol);
    }

    [Fact]
    public async Task GetTopLosersAsync_ReturnsSortedByChange24hAscending()
    {
        using var db = CreateDb(nameof(GetTopLosersAsync_ReturnsSortedByChange24hAscending));
        var repo = new CryptoPriceRepository(db);

        var at = DateTime.UtcNow;
        await repo.AddRangeAsync(new[]
        {
            MakePrice("a", "A", 5m, at),
            MakePrice("b", "B", -8m, at),
            MakePrice("c", "C", -2m, at)
        });

        var losers = (await repo.GetTopLosersAsync(2)).ToList();

        Assert.Equal(2, losers.Count);
        Assert.Equal("B", losers[0].Symbol);
        Assert.Equal("C", losers[1].Symbol);
    }

    [Fact]
    public async Task GetHistoryBySymbolAsync_ReturnsAllRecordsForSymbol()
    {
        using var db = CreateDb(nameof(GetHistoryBySymbolAsync_ReturnsAllRecordsForSymbol));
        var repo = new CryptoPriceRepository(db);

        var t1 = DateTime.UtcNow.AddMinutes(-5);
        var t2 = DateTime.UtcNow;

        await repo.AddRangeAsync(new[]
        {
            MakePrice("bitcoin", "BTC", 1m, t1),
            MakePrice("bitcoin", "BTC", 3m, t2),
            MakePrice("ethereum", "ETH", 2m, t2)
        });

        var history = (await repo.GetHistoryBySymbolAsync("BTC")).ToList();

        Assert.Equal(2, history.Count);
        Assert.All(history, p => Assert.Equal("BTC", p.Symbol));
    }

    [Fact]
    public async Task GetTotalCountAsync_ReturnsCorrectCount()
    {
        using var db = CreateDb(nameof(GetTotalCountAsync_ReturnsCorrectCount));
        var repo = new CryptoPriceRepository(db);

        var at = DateTime.UtcNow;
        await repo.AddRangeAsync(new[]
        {
            MakePrice("bitcoin", "BTC", 1m, at),
            MakePrice("ethereum", "ETH", 2m, at)
        });

        Assert.Equal(2, await repo.GetTotalCountAsync());
    }
}
