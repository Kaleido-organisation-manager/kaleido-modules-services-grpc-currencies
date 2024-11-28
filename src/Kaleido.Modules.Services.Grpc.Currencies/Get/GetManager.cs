using AutoMapper;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Currencies.Get;

public class GetManager : IGetManager
{
    private readonly IMapper _mapper;
    private readonly IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity> _currencyLifeCycleHandler;
    private readonly IEntityLifecycleHandler<DenominationEntity, DenominationRevisionEntity> _denominationLifeCycleHandler;
    private readonly ILogger<GetManager> _logger;

    public GetManager(
        IMapper mapper,
        IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity> currencyLifeCycleHandler,
        IEntityLifecycleHandler<DenominationEntity, DenominationRevisionEntity> denominationLifeCycleHandler,
        ILogger<GetManager> logger
    )
    {
        _mapper = mapper;
        _currencyLifeCycleHandler = currencyLifeCycleHandler;
        _denominationLifeCycleHandler = denominationLifeCycleHandler;
        _logger = logger;
    }

    public async Task<ManagerResponse> GetAsync(Guid key, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to get currency with key: {Key}", key);

        var currency = await _currencyLifeCycleHandler.GetAsync(key, cancellationToken: cancellationToken);

        if (currency == null || currency.Revision?.Action == RevisionAction.Deleted)
        {
            _logger.LogInformation("Currency with key: {Key} not found or already deleted.", key);
            return ManagerResponse.NotFound();
        }

        var denominations = await _denominationLifeCycleHandler.FindAllAsync(
            denomination => denomination.CurrencyKey == key,
            revision => revision.Status == RevisionStatus.Active && revision.Action != RevisionAction.Deleted,
            cancellationToken: cancellationToken
        );

        _logger.LogInformation("Currency with key: {Key} and its denominations have been successfully retrieved.", key);
        _logger.LogInformation("Resolved {DenominationsCount} denominations for currency with key: {Key}", denominations.Count(), key);

        return ManagerResponse.Success(currency, denominations);
    }
}
