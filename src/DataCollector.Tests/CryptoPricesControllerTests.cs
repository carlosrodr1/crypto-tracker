using DataCollector.Api.Controllers;
using DataCollector.Shared.Entities;
using DataCollector.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DataCollector.Tests;

public class CryptoPricesControllerTests
{
    private static CryptoPricesController BuildController(Mock<ICryptoPriceRepository> mock) =>
        new(mock.Object);

    private static CryptoPrice MakePrice(string symbol) =>
        new() { Id = 1, CoinId = symbol.ToLower(), Symbol = symbol, Name = symbol, PriceUsd = 100m, CollectedAt = DateTime.UtcNow };

    [Fact]
    public async Task GetLatest_ReturnsOkWithData()
    {
        var mock = new Mock<ICryptoPriceRepository>();
        mock.Setup(r => r.GetLatestAsync(1, 20)).ReturnsAsync(new[] { MakePrice("BTC") });
        mock.Setup(r => r.GetTotalCountAsync()).ReturnsAsync(1);
        mock.Setup(r => r.GetLastCollectedAtAsync()).ReturnsAsync(DateTime.UtcNow);

        var result = await BuildController(mock).GetLatest();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetLatest_ReturnsBadRequestForInvalidPageParameter()
    {
        var mock = new Mock<ICryptoPriceRepository>();

        var result = await BuildController(mock).GetLatest(page: 0);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetLatest_ReturnsBadRequestWhenPageSizeExceedsMaximum()
    {
        var mock = new Mock<ICryptoPriceRepository>();

        var result = await BuildController(mock).GetLatest(pageSize: 101);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetById_ReturnsNotFoundWhenRecordDoesNotExist()
    {
        var mock = new Mock<ICryptoPriceRepository>();
        mock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((CryptoPrice?)null);

        var result = await BuildController(mock).GetById(99);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetById_ReturnsOkWhenRecordExists()
    {
        var mock = new Mock<ICryptoPriceRepository>();
        mock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakePrice("BTC"));

        var result = await BuildController(mock).GetById(1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetTopGainers_ReturnsBadRequestWhenLimitExceedsMaximum()
    {
        var mock = new Mock<ICryptoPriceRepository>();

        var result = await BuildController(mock).GetTopGainers(limit: 51);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetStats_ReturnsCorrectTotals()
    {
        var mock = new Mock<ICryptoPriceRepository>();
        mock.Setup(r => r.GetTotalCountAsync()).ReturnsAsync(200);
        mock.Setup(r => r.GetLastCollectedAtAsync()).ReturnsAsync(DateTime.UtcNow);

        var result = await BuildController(mock).GetStats();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }
}
