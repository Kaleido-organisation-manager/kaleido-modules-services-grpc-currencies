using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Currencies.Create;

public class CreateManager : ICreateManager
{
    private readonly IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity> _currencyLifeCycleHandler;
    private readonly IEntityLifecycleHandler<DenominationEntity, DenominationRevisionEntity> _denominationLifeCycleHandler;
    private readonly ILogger<CreateManager> _logger;

    public CreateManager(
        IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity> currencyRepository,
        IEntityLifecycleHandler<DenominationEntity, DenominationRevisionEntity> denominationRepository,
        ILogger<CreateManager> logger
        )
    {
        _currencyLifeCycleHandler = currencyRepository;
        _denominationLifeCycleHandler = denominationRepository;
        _logger = logger;
    }

    public async Task<ManagerResponse> CreateAsync(CurrencyEntity currency, IEnumerable<DenominationEntity> denominations, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating currency with name: {Name}", currency.Name);
        var timestamp = DateTime.UtcNow;

        var currencyRevision = new CurrencyRevisionEntity
        {
            Key = Guid.NewGuid(),
            CreatedAt = timestamp
        };

        var currencyResult = await _currencyLifeCycleHandler.CreateAsync(currency, currencyRevision, cancellationToken: cancellationToken);

        var storedDenominations = new List<EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>>();

        foreach (var denomination in denominations)
        {
            var denominationRevision = new DenominationRevisionEntity
            {
                Key = Guid.NewGuid(),
                CreatedAt = timestamp
            };
            denomination.CurrencyKey = currencyResult.Key;

            var denominationResult = await _denominationLifeCycleHandler.CreateAsync(denomination, denominationRevision, cancellationToken: cancellationToken);
            storedDenominations.Add(denominationResult);
        }

        _logger.LogInformation("Currency with key: {Key} and its denominations have been successfully created.", currencyResult.Key);

        return ManagerResponse.Success(currencyResult, storedDenominations);
    }
}
