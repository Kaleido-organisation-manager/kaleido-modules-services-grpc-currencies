using Kaleido.Common.Services.Grpc.Configuration.Constants;
using Kaleido.Common.Services.Grpc.Configuration.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Kaleido.Modules.Services.Grpc.Currencies.Common.Configuration;

public class CurrencyEntityRevisionDbContext : DbContext, IKaleidoDbContext<CurrencyRevisionEntity>
{
    public DbSet<CurrencyRevisionEntity> Items { get; set; } = null!;

    public CurrencyEntityRevisionDbContext(DbContextOptions<CurrencyEntityRevisionDbContext> options)
    : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CurrencyRevisionEntity>(entity =>
        {
            entity.ToTable("CurrencyRevisions");

            DefaultOnModelCreatingMethod.ForBaseRevisionEntity(entity);
        });
    }
}