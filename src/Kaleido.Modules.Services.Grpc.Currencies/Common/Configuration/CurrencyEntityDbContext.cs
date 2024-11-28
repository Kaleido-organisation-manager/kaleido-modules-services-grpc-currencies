
using Kaleido.Common.Services.Grpc.Configuration.Constants;
using Kaleido.Common.Services.Grpc.Configuration.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Kaleido.Modules.Services.Grpc.Currencies.Common.Configuration;

public class CurrencyEntityDbContext : DbContext, IKaleidoDbContext<CurrencyEntity>
{
    public DbSet<CurrencyEntity> Items { get; set; } = null!;

    public CurrencyEntityDbContext(DbContextOptions<CurrencyEntityDbContext> options)
    : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CurrencyEntity>(entity =>
        {
            entity.ToTable("Currencies");

            entity.Property(c => c.Name).IsRequired().HasColumnType("varchar(100)");
            entity.Property(c => c.Code).IsRequired().HasColumnType("varchar(3)");
            entity.Property(c => c.Symbol).HasColumnType("varchar(10)");

            DefaultOnModelCreatingMethod.ForBaseEntity(entity);
        });
    }
}