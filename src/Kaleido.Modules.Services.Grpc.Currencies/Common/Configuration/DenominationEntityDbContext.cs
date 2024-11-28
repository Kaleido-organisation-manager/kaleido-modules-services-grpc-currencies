using Kaleido.Common.Services.Grpc.Configuration.Constants;
using Kaleido.Common.Services.Grpc.Configuration.Interfaces;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Kaleido.Modules.Services.Grpc.Currencies.Common.Configuration;

public class DenominationEntityDbContext : DbContext, IKaleidoDbContext<DenominationEntity>
{
    public DbSet<DenominationEntity> Items { get; set; } = null!;

    public DenominationEntityDbContext(DbContextOptions<DenominationEntityDbContext> options)
    : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DenominationEntity>(entity =>
        {
            entity.ToTable("Denominations");
            entity.Property(d => d.CurrencyKey).IsRequired().HasColumnType("varchar(36)");
            entity.Property(d => d.Value).IsRequired().HasColumnType("decimal(18, 2)");
            entity.Property(d => d.Description).IsRequired().HasColumnType("varchar(100)");
            DefaultOnModelCreatingMethod.ForBaseEntity(entity);
        });
    }
}
