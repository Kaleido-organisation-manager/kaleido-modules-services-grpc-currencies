using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using static Kaleido.Grpc.Currencies.GrpcCurrencies;

namespace Kaleido.Modules.Services.Grpc.Currencies.Client.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCurrencyClient(this IServiceCollection services, string connectionString)
    {
        var channel = GrpcChannel.ForAddress(connectionString);
        var client = new GrpcCurrenciesClient(channel);
        services.AddSingleton(client);
        return services;
    }
}
