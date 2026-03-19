using DataCollector.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DataCollector.Api.Controllers;

[ApiController]
[Route("api/crypto")]
public class CryptoPricesController : ControllerBase
{
    private readonly ICryptoPriceRepository _repository;

    public CryptoPricesController(ICryptoPriceRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetLatest([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { error = "Invalid pagination parameters. page >= 1, 1 <= pageSize <= 100." });

        var prices = await _repository.GetLatestAsync(page, pageSize);
        var total = await _repository.GetTotalCountAsync();
        var lastCollectedAt = await _repository.GetLastCollectedAtAsync();

        return Ok(new
        {
            data = prices,
            page,
            pageSize,
            total,
            lastCollectedAt
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var price = await _repository.GetByIdAsync(id);
        return price is null ? NotFound(new { error = $"Record {id} not found." }) : Ok(price);
    }

    [HttpGet("history/{symbol}")]
    public async Task<IActionResult> GetHistory(string symbol)
    {
        var history = await _repository.GetHistoryBySymbolAsync(symbol);
        return Ok(history);
    }

    [HttpGet("top-gainers")]
    public async Task<IActionResult> GetTopGainers([FromQuery] int limit = 10)
    {
        if (limit < 1 || limit > 50)
            return BadRequest(new { error = "limit must be between 1 and 50." });

        var gainers = await _repository.GetTopGainersAsync(limit);
        return Ok(gainers);
    }

    [HttpGet("top-losers")]
    public async Task<IActionResult> GetTopLosers([FromQuery] int limit = 10)
    {
        if (limit < 1 || limit > 50)
            return BadRequest(new { error = "limit must be between 1 and 50." });

        var losers = await _repository.GetTopLosersAsync(limit);
        return Ok(losers);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var total = await _repository.GetTotalCountAsync();
        var lastCollectedAt = await _repository.GetLastCollectedAtAsync();

        return Ok(new
        {
            totalRecords = total,
            lastCollectedAt,
            checkedAt = DateTimeOffset.UtcNow
        });
    }
}
