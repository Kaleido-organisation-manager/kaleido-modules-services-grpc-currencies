using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Currencies.Create;

public class CreateManager : ICreateManager
{
    private readonly IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity> _currencyLifeCycleHandler;
    private readonly ILogger<CreateManager> _logger;

    public CreateManager(
        IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity> currencyRepository,
        ILogger<CreateManager> logger
        )
    {
        _currencyLifeCycleHandler = currencyRepository;
        _logger = logger;
    }

    public async Task<ManagerResponse> CreateAsync(CurrencyEntity currency, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating currency with name: {Name}", currency.Name);

        var result = await _currencyLifeCycleHandler.CreateAsync(currency, cancellationToken: cancellationToken);
        return ManagerResponse.Success(result);
    }
}
