using DataCollector.Worker.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Text;
using Xunit;

namespace DataCollector.Tests;

public class CryptoPriceServiceTests
{
    private const string ValidCoinGeckoJson = """
        [
          {
            "id": "bitcoin",
            "symbol": "btc",
            "name": "Bitcoin",
            "current_price": 50000.0,
            "price_change_percentage_24h": 2.5,
            "market_cap": 1000000000000,
            "total_volume": 30000000000
          },
          {
            "id": "ethereum",
            "symbol": "eth",
            "name": "Ethereum",
            "current_price": 3000.0,
            "price_change_percentage_24h": -1.2,
            "market_cap": 360000000000,
            "total_volume": 15000000000
          }
        ]
        """;

    private static IConfiguration BuildConfig(string url = "https://api.coingecko.com/api/v3") =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Scraper:TargetUrl"] = url })
            .Build();

    private static IHttpClientFactory BuildFactory(string content, HttpStatusCode status = HttpStatusCode.OK)
    {
        var handler = new FakeHttpMessageHandler(content, status);
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.coingecko.com") };
        var mock = new Mock<IHttpClientFactory>();
        mock.Setup(f => f.CreateClient("scraper")).Returns(client);
        return mock.Object;
    }

    [Fact]
    public async Task ScrapeAsync_ParsesSymbolNameAndPriceFromValidJson()
    {
        var service = new CryptoPriceService(
            BuildFactory(ValidCoinGeckoJson),
            new Mock<ILogger<CryptoPriceService>>().Object,
            BuildConfig());

        var result = await service.ScrapeAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("BTC", result[0].Symbol);
        Assert.Equal("Bitcoin", result[0].Name);
        Assert.Equal(50000m, result[0].PriceUsd);
        Assert.Equal(2.5m, result[0].Change24h);
    }

    [Fact]
    public async Task ScrapeAsync_AllItemsShareTheSameCollectedAt()
    {
        var service = new CryptoPriceService(
            BuildFactory(ValidCoinGeckoJson),
            new Mock<ILogger<CryptoPriceService>>().Object,
            BuildConfig());

        var result = await service.ScrapeAsync();

        Assert.Equal(result[0].CollectedAt, result[1].CollectedAt);
    }

    [Fact]
    public async Task ScrapeAsync_ReturnsEmptyListOnEmptyJsonArray()
    {
        var service = new CryptoPriceService(
            BuildFactory("[]"),
            new Mock<ILogger<CryptoPriceService>>().Object,
            BuildConfig());

        var result = await service.ScrapeAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task ScrapeAsync_ReturnsEmptyListOnInvalidJson()
    {
        var service = new CryptoPriceService(
            BuildFactory("<html>not json</html>"),
            new Mock<ILogger<CryptoPriceService>>().Object,
            BuildConfig());

        var result = await service.ScrapeAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task ScrapeAsync_ReturnsEmptyListOnHttpError()
    {
        var service = new CryptoPriceService(
            BuildFactory("", HttpStatusCode.ServiceUnavailable),
            new Mock<ILogger<CryptoPriceService>>().Object,
            BuildConfig());

        var result = await service.ScrapeAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task ScrapeAsync_SymbolIsUppercased()
    {
        var service = new CryptoPriceService(
            BuildFactory(ValidCoinGeckoJson),
            new Mock<ILogger<CryptoPriceService>>().Object,
            BuildConfig());

        var result = await service.ScrapeAsync();

        Assert.All(result, p => Assert.Equal(p.Symbol, p.Symbol.ToUpperInvariant()));
    }
}

internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly string _content;
    private readonly HttpStatusCode _statusCode;

    public FakeHttpMessageHandler(string content, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _content = content;
        _statusCode = statusCode;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
        Task.FromResult(new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_content, Encoding.UTF8, "application/json")
        });
}
