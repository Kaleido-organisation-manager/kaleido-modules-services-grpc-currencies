using Kaleido.Common.Services.Grpc.Exceptions;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Currencies.Delete;

public class DeleteManager : IDeleteManager
{
    private readonly IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity> _currencyLifeCycleHandler;
    private readonly ILogger<DeleteManager> _logger;

    public DeleteManager(
        IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity> currencyLifeCycleHandler,
        ILogger<DeleteManager> logger
        )
    {
        _currencyLifeCycleHandler = currencyLifeCycleHandler;
        _logger = logger;
    }

    public async Task<ManagerResponse> DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        var currencyKey = Guid.Parse(key);
        _logger.LogInformation("Deleting currency with key: {CurrencyKey}", currencyKey);
        try
        {
            var result = await _currencyLifeCycleHandler.DeleteAsync(currencyKey, cancellationToken: cancellationToken);
            return ManagerResponse.Success(result);
        }
        catch (RevisionNotFoundException)
        {
            return ManagerResponse.NotFound();
        }
    }
}