using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Currencies.GetAllRevisions;

public class GetAllRevisionsManager : IGetAllRevisionsManager
{
    private readonly IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity> _lifeCycleHandler;
    private readonly ILogger<GetAllRevisionsManager> _logger;

    public GetAllRevisionsManager(
        IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity> lifeCycleHandler,
        ILogger<GetAllRevisionsManager> logger
        )
    {
        _lifeCycleHandler = lifeCycleHandler;
        _logger = logger;
    }

    public async Task<IEnumerable<ManagerResponse>> GetAllRevisionsAsync(Guid key, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("getting all revisions for currency with key: {Key}", key);
        var revisions = await _lifeCycleHandler.GetAllAsync(key, cancellationToken);
        return revisions.Select(r => ManagerResponse.Success(r));
    }
}
