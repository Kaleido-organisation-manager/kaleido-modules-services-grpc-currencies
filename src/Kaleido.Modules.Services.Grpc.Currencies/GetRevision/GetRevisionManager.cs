using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Currencies.GetRevision;

public class GetRevisionManager : IGetRevisionManager
{
    private readonly IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity> _lifeCycleHandler;
    private readonly ILogger<GetRevisionManager> _logger;

    public GetRevisionManager(
        IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity> lifecycleHandler,
        ILogger<GetRevisionManager> logger
    )
    {
        _lifeCycleHandler = lifecycleHandler;
        _logger = logger;
    }

    public async Task<ManagerResponse> GetRevisionAsync(Guid key, DateTime createdAt, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting revision for currency with key: {Key} at {CreatedAt}", key, createdAt);

        var currencyRevision = await _lifeCycleHandler.GetHistoricAsync(key, createdAt, cancellationToken);

        if (currencyRevision is null)
        {
            return ManagerResponse.NotFound();
        }

        return ManagerResponse.Success(currencyRevision);
    }
}
