using Kaleido.Common.Services.Grpc.Exceptions;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Currencies.Update;

public class UpdateManager : IUpdateManager
{

    private readonly IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity> _lifeCycleHandler;
    private readonly ILogger<UpdateManager> _logger;

    public UpdateManager(
        IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity> lifecycleHandler,
        ILogger<UpdateManager> logger
    )
    {
        _lifeCycleHandler = lifecycleHandler;
        _logger = logger;
    }

    public async Task<ManagerResponse> UpdateAsync(Guid key, CurrencyEntity currency, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating currency with key: {Key}", key);

        try
        {
            var result = await _lifeCycleHandler.UpdateAsync(key, currency, cancellationToken: cancellationToken);
            return ManagerResponse.Success(result);
        }
        catch (Exception ex) when (ex is RevisionNotFoundException or EntityNotFoundException)
        {
            return ManagerResponse.NotFound();
        }
    }
}
