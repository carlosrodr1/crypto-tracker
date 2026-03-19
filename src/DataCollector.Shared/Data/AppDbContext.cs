using DataCollector.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataCollector.Shared.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<CryptoPrice> CryptoPrices => Set<CryptoPrice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CryptoPrice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CoinId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Symbol).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => new { e.Symbol, e.CollectedAt });
        });
    }
}
