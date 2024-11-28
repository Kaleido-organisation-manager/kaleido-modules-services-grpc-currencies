using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Currencies.GetRevision;

public class GetRevisionManager : IGetRevisionManager
{
    private readonly IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity> _lifeCycleHandler;
    private readonly IEntityLifecycleHandler<DenominationEntity, DenominationRevisionEntity> _denominationLifeCycleHandler;
    private readonly ILogger<GetRevisionManager> _logger;

    public GetRevisionManager(
        IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity> lifecycleHandler,
        IEntityLifecycleHandler<DenominationEntity, DenominationRevisionEntity> denominationLifecycleHandler,
        ILogger<GetRevisionManager> logger
    )
    {
        _lifeCycleHandler = lifecycleHandler;
        _denominationLifeCycleHandler = denominationLifecycleHandler;
        _logger = logger;
    }

    public async Task<ManagerResponse> GetRevisionAsync(Guid key, DateTime createdAt, CancellationToken cancellationToken = default)
    {
        var currencyRevision = await _lifeCycleHandler.GetHistoricAsync(key, createdAt, cancellationToken);

        if (currencyRevision == null)
        {
            return ManagerResponse.NotFound();
        }

        var revisionTimestamp = currencyRevision.Revision.CreatedAt;
        var denominations = await _denominationLifeCycleHandler.FindAllAsync(
            denomination => denomination.CurrencyKey == key,
            r => r.CreatedAt == revisionTimestamp,
            cancellationToken: cancellationToken);

        return ManagerResponse.Success(currencyRevision, denominations);
    }
}
