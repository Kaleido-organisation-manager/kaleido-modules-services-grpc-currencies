using Kaleido.Common.Services.Grpc.Configuration.Constants;
using Kaleido.Common.Services.Grpc.Configuration.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Microsoft.EntityFrameworkCore;

namespace Kaleido.Modules.Services.Grpc.Currencies.Common.Configuration;

public class CurrencyEntityRevisionDbContext : DbContext, IKaleidoDbContext<BaseRevisionEntity>
{
    public DbSet<BaseRevisionEntity> Items { get; set; }

    public CurrencyEntityRevisionDbContext(DbContextOptions<CurrencyEntityRevisionDbContext> options)
    : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BaseRevisionEntity>(entity =>
        {
            entity.ToTable("CurrencyRevisions");
            DefaultOnModelCreatingMethod.ForBaseEntity(entity);
            DefaultOnModelCreatingMethod.ForBaseRevisionEntity(entity);
        });
    }
}