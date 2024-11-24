using Kaleido.Common.Services.Grpc.Configuration.Constants;
using Kaleido.Common.Services.Grpc.Configuration.Interfaces;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Kaleido.Modules.Services.Grpc.Currencies.Common.Configuration;

public class DenominationEntityRevisionDbContext : DbContext, IKaleidoDbContext<DenominationRevisionEntity>
{
    public DbSet<DenominationRevisionEntity> Items { get; set; } = null!;

    public DenominationEntityRevisionDbContext(DbContextOptions<DenominationEntityRevisionDbContext> options)
    : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DenominationRevisionEntity>(entity =>
        {
            entity.ToTable("Denominations");
            DefaultOnModelCreatingMethod.ForBaseEntity(entity);
            DefaultOnModelCreatingMethod.ForBaseRevisionEntity(entity);
        });
    }
}
