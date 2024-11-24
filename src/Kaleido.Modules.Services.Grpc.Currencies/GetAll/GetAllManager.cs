using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Currencies.GetAll;

public class GetAllManager : IGetAllManager
{
    private readonly IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity> _currencyLifeCycleHandler;
    private readonly IEntityLifecycleHandler<DenominationEntity, DenominationRevisionEntity> _denominationLifeCycleHandler;
    private readonly ILogger<GetAllManager> _logger;

    public GetAllManager(
        IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity> currencyLifeCycleHandler,
        IEntityLifecycleHandler<DenominationEntity, DenominationRevisionEntity> denominationLifeCycleHandler,
        ILogger<GetAllManager> logger
         )
    {
        _currencyLifeCycleHandler = currencyLifeCycleHandler;
        _denominationLifeCycleHandler = denominationLifeCycleHandler;
        _logger = logger;
    }

    public async Task<IEnumerable<ManagerResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var currencies = await _currencyLifeCycleHandler.FindAllAsync(
            c => true,
            r => r.Action != RevisionAction.Deleted && r.Status == RevisionStatus.Active,
            cancellationToken: cancellationToken
        );

        var result = new List<ManagerResponse>();

        foreach (var currency in currencies)
        {
            var denominations = await _denominationLifeCycleHandler.FindAllAsync(
                d => d.CurrencyKey == currency.Key,
                r => r.Status == RevisionStatus.Active && r.Action != RevisionAction.Deleted,
                cancellationToken: cancellationToken
            );
            result.Add(ManagerResponse.Success(currency, denominations));
        }

        return result;
    }
}
