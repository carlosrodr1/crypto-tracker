using DataCollector.Shared.Entities;
using Polly.CircuitBreaker;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataCollector.Worker.Services;

public class CryptoPriceService : IScraperService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CryptoPriceService> _logger;
    private readonly string _baseUrl;
    private const int CoinsPerPage = 50;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CryptoPriceService(
        IHttpClientFactory httpClientFactory,
        ILogger<CryptoPriceService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient("scraper");
        _logger = logger;
        _baseUrl = configuration["Scraper:TargetUrl"] ?? "https://api.coingecko.com/api/v3";
    }

    public async Task<IReadOnlyList<CryptoPrice>> ScrapeAsync(CancellationToken cancellationToken = default)
    {
        var url = $"{_baseUrl}/coins/markets?vs_currency=usd&order=market_cap_desc&per_page={CoinsPerPage}&page=1&sparkline=false";
        _logger.LogInformation("Fetching crypto prices from {Url}", url);

        string json;
        try
        {
            json = await _httpClient.GetStringAsync(url, cancellationToken);
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(ex, "Circuit breaker is open. Halting scrape cycle.");
            return Array.Empty<CryptoPrice>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch crypto prices after all retries.");
            return Array.Empty<CryptoPrice>();
        }

        List<CoinGeckoMarket>? markets;
        try
        {
            markets = JsonSerializer.Deserialize<List<CoinGeckoMarket>>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize CoinGecko response. The API format may have changed.");
            return Array.Empty<CryptoPrice>();
        }

        if (markets is null || markets.Count == 0)
        {
            _logger.LogWarning("CoinGecko returned an empty or null response.");
            return Array.Empty<CryptoPrice>();
        }

        var collectedAt = DateTime.UtcNow;
        var prices = markets
            .Select(m => new CryptoPrice
            {
                CoinId = m.Id,
                Symbol = m.Symbol.ToUpperInvariant(),
                Name = m.Name,
                PriceUsd = m.CurrentPrice,
                Change24h = m.PriceChangePercentage24h,
                MarketCap = m.MarketCap,
                Volume24h = m.TotalVolume,
                CollectedAt = collectedAt
            })
            .ToList()
            .AsReadOnly();

        _logger.LogInformation("Fetched {Count} coins from CoinGecko.", prices.Count);
        return prices;
    }

    private sealed record CoinGeckoMarket(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("symbol")] string Symbol,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("current_price")] decimal CurrentPrice,
        [property: JsonPropertyName("price_change_percentage_24h")] decimal PriceChangePercentage24h,
        [property: JsonPropertyName("market_cap")] decimal MarketCap,
        [property: JsonPropertyName("total_volume")] decimal TotalVolume
    );
}
