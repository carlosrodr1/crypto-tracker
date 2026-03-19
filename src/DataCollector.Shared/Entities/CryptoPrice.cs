namespace DataCollector.Shared.Entities;

public class CryptoPrice
{
    public int Id { get; set; }
    public string CoinId { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal PriceUsd { get; set; }
    public decimal Change24h { get; set; }
    public decimal MarketCap { get; set; }
    public decimal Volume24h { get; set; }
    public DateTime CollectedAt { get; set; }
}
