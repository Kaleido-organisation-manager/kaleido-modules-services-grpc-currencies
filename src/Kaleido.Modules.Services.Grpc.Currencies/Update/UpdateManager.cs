using AutoMapper;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Exceptions;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;
using Microsoft.Extensions.Logging;

namespace Kaleido.Modules.Services.Grpc.Currencies.Update;

public class UpdateManager : IUpdateManager
{

    private readonly IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity> _lifeCycleHandler;
    private readonly IEntityLifecycleHandler<DenominationEntity, DenominationRevisionEntity> _denominationLifeCycleHandler;
    private readonly ILogger<UpdateManager> _logger;
    private readonly IMapper _mapper;

    public UpdateManager(
        IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity> lifecycleHandler,
        IEntityLifecycleHandler<DenominationEntity, DenominationRevisionEntity> denominationLifecycleHandler,
        ILogger<UpdateManager> logger,
        IMapper mapper
    )
    {
        _lifeCycleHandler = lifecycleHandler;
        _denominationLifeCycleHandler = denominationLifecycleHandler;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<ManagerResponse> UpdateAsync(
        Guid key,
        CurrencyEntity currency,
        IEnumerable<DenominationEntity> denominations,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting UpdateAsync with key: {Key}", key);

        var timestamp = DateTime.UtcNow;

        var currencyRevision = new CurrencyRevisionEntity
        {
            Key = key,
            CreatedAt = timestamp,
        };

        EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity>? currencyResult;
        try
        {
            currencyResult = await _lifeCycleHandler.UpdateAsync(key, currency, currencyRevision, cancellationToken);
            _logger.LogInformation("Currency update successful for key: {Key}", key);
        }
        catch (NotModifiedException)
        {
            _logger.LogInformation("Currency update not modified for key: {Key}", key);
            currencyResult = await _lifeCycleHandler.GetAsync(key, cancellationToken: cancellationToken);
        }
        catch (Exception ex) when (ex is RevisionNotFoundException or EntityNotFoundException)
        {
            _logger.LogError(ex, "Currency or revision not found for key: {Key}", key);
            return ManagerResponse.NotFound();
        }

        if (currencyResult == null)
        {
            _logger.LogError("Currency result is null for key: {Key}", key);
            return ManagerResponse.NotFound();
        }

        var activeDenominations = await _denominationLifeCycleHandler.FindAllAsync(
            denomination => denomination.CurrencyKey == key,
            revision => revision.Status == RevisionStatus.Active,
            cancellationToken: cancellationToken);

        // Get Prices to delete
        var denominationsToDelete = activeDenominations
            .Where(x => !denominations.Any(y => y.Value == x.Entity.Value) &&
                        x.Revision.Action != RevisionAction.Deleted)
            .ToList();

        // Get Prices to create
        var denominationsToCreate = denominations
            .Where(x => !activeDenominations
                .Any(y => y.Entity.Value == x.Value))
            .Where(x => activeDenominations.FirstOrDefault(y => y.Entity.Value == x.Value)?.Revision.Action != RevisionAction.Deleted)
            .ToList();

        // Get Prices to restore
        var denominationsToRestore = activeDenominations
            .Where(x => denominations
                .Any(y =>
                    y.Value == x.Entity.Value &&
                    x.Revision.Action == RevisionAction.Deleted))
            .ToList();

        // Get Prices to update
        var denominationsToUpdate = denominations.Where(x =>
            activeDenominations.Any(y =>
                y.Entity.Value == x.Value &&
                !y.Entity.Equals(x) &&
                y.Revision.Action != RevisionAction.Deleted))
            .Select(x =>
            {
                var matchedActiveDenomination = activeDenominations.First(y => y.Entity.Value == x.Value);
                var copyOfMatched = _mapper.Map<EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>>(matchedActiveDenomination);
                x.CurrencyKey = key;
                copyOfMatched.Entity = x;
                return copyOfMatched;
            })
            .ToList();

        // Get the unchanged prices
        var unchangedDenominations = activeDenominations
            .Where(x => !denominationsToDelete.Any(y => y.Key == x.Key) &&
                        !denominationsToCreate.Any(y => y.Value == x.Entity.Value) &&
                        !denominationsToRestore.Any(y => y.Key == x.Key) &&
                        !denominationsToUpdate.Any(y => y.Key == x.Key) &&
                        x.Revision.Action != RevisionAction.Deleted)
            .ToList();

        var updatedDenominations = new List<EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>>();

        // Delete prices
        foreach (var denomination in denominationsToDelete)
        {
            var denominationRevisionEntity = new DenominationRevisionEntity
            {
                Key = denomination.Key,
                CreatedAt = timestamp,
            };

            var denominationResult = await _denominationLifeCycleHandler.DeleteAsync(denomination.Key, denominationRevisionEntity, cancellationToken);
            updatedDenominations.Add(denominationResult);
            _logger.LogInformation("Deleted denomination with key: {Key}", denomination.Key);
        }

        // Create prices
        foreach (var denomination in denominationsToCreate)
        {
            denomination.CurrencyKey = key;
            var denominationRevisionEntity = new DenominationRevisionEntity
            {
                Key = Guid.NewGuid(),
                CreatedAt = timestamp,
            };

            var denominationResult = await _denominationLifeCycleHandler.CreateAsync(denomination, denominationRevisionEntity, cancellationToken);
            updatedDenominations.Add(denominationResult);
            _logger.LogInformation("Created denomination with value: {Value}", denomination.Value);
        }

        // Restore prices
        foreach (var denomination in denominationsToRestore)
        {
            var denominationRevisionEntity = new DenominationRevisionEntity
            {
                Key = denomination.Key,
                CreatedAt = timestamp,
            };

            var denominationResult = await _denominationLifeCycleHandler.RestoreAsync(denomination.Key, denominationRevisionEntity, cancellationToken);
            updatedDenominations.Add(denominationResult);
            _logger.LogInformation("Restored denomination with key: {Key}", denomination.Key);
        }

        // Update prices
        foreach (var denomination in denominationsToUpdate)
        {
            var denominationRevisionEntity = new DenominationRevisionEntity
            {
                Key = denomination.Key,
                CreatedAt = timestamp,
            };

            var denominationResult = await _denominationLifeCycleHandler.UpdateAsync(denomination.Key, denomination.Entity, denominationRevisionEntity, cancellationToken);
            updatedDenominations.Add(denominationResult);
            _logger.LogInformation("Updated denomination with key: {Key}", denomination.Key);
        }

        // Update the unchanged prices
        updatedDenominations.AddRange(unchangedDenominations.Select(x =>
        {
            x.Revision.Action = RevisionAction.Unmodified;
            return x;
        }));

        return ManagerResponse.Success(currencyResult, updatedDenominations);
    }
}
