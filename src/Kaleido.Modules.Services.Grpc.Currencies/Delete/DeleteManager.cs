using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Exceptions;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Currencies.Delete;

public class DeleteManager : IDeleteManager
{
    private readonly IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity> _currencyLifeCycleHandler;
    private readonly IEntityLifecycleHandler<DenominationEntity, DenominationRevisionEntity> _denominationLifeCycleHandler;
    private readonly ILogger<DeleteManager> _logger;

    public DeleteManager(
        IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity> currencyLifeCycleHandler,
        IEntityLifecycleHandler<DenominationEntity, DenominationRevisionEntity> denominationLifeCycleHandler,
        ILogger<DeleteManager> logger
        )
    {
        _currencyLifeCycleHandler = currencyLifeCycleHandler;
        _denominationLifeCycleHandler = denominationLifeCycleHandler;
        _logger = logger;
    }

    public async Task<ManagerResponse> DeleteAsync(Guid key, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to delete currency with key: {Key}", key);

        EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity>? requestedCurrency;
        try
        {
            requestedCurrency = await _currencyLifeCycleHandler.GetAsync(key, cancellationToken: cancellationToken);
        }
        catch (Exception ex) when (ex is EntityNotFoundException or RevisionNotFoundException)
        {
            _logger.LogError(ex, "Failed to find currency with key: {Key}. Entity or Revision not found.", key);
            return ManagerResponse.NotFound();
        }

        if (requestedCurrency == null || requestedCurrency.Revision.Action == RevisionAction.Deleted)
        {
            _logger.LogInformation("Currency with key: {Key} not found or already deleted.", key);
            return ManagerResponse.NotFound();
        }

        var denominations = await _denominationLifeCycleHandler.FindAllAsync(
            d => d.CurrencyKey == key,
            revision => revision.Status == RevisionStatus.Active && revision.Action != RevisionAction.Deleted,
            cancellationToken: cancellationToken);

        var timestamp = DateTime.UtcNow;

        var resultDenominations = new List<EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>>();
        foreach (var denomination in denominations)
        {
            var denominationRevision = new DenominationRevisionEntity
            {
                Key = denomination.Key,
                CreatedAt = timestamp
            };

            var result = await _denominationLifeCycleHandler.DeleteAsync(denomination.Key, denominationRevision, cancellationToken: cancellationToken);
            if (result != null)
            {
                resultDenominations.Add(result);
            }
        }

        var currencyRevision = new CurrencyRevisionEntity
        {
            Key = key,
            CreatedAt = timestamp
        };
        var currencyResult = await _currencyLifeCycleHandler.DeleteAsync(key, currencyRevision, cancellationToken: cancellationToken);

        _logger.LogInformation("Currency with key: {Key} and its denominations have been successfully deleted.", key);

        return ManagerResponse.Success(currencyResult, resultDenominations);
    }
}