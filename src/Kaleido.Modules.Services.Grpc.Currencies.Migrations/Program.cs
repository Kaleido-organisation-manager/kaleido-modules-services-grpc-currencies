using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Kaleido.Common.Services.Grpc.Configuration.Extensions;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Configuration;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureAppConfiguration((hostingContext, config) =>
{
    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    config.AddJsonFile($"appsettings.Development.json", optional: true, reloadOnChange: true);
    config.AddEnvironmentVariables();
});

builder.ConfigureServices((hostContext, services) =>
{
    var connectionString = hostContext.Configuration.GetConnectionString("Currencies");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new ArgumentNullException(nameof(connectionString), "Expected a value for the currencies db connection string");
    }
    var assemblyName = "Kaleido.Modules.Services.Grpc.Currencies.Migrations";
    services.AddKaleidoMigrationEntityDbContext<CurrencyEntity, CurrencyEntityDbContext>(connectionString, assemblyName);
    services.AddKaleidoMigrationRevisionDbContext<BaseRevisionEntity, CurrencyEntityRevisionDbContext>(connectionString, assemblyName);

});

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var entityContext = services.GetRequiredService<CurrencyEntityDbContext>();
    var revisionContext = services.GetRequiredService<CurrencyEntityRevisionDbContext>();

    await entityContext.Database.MigrateAsync();
    await revisionContext.Database.MigrateAsync();

    Console.WriteLine("Migration completed successfully.");
}
