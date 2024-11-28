using AutoMapper;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Currencies.GetAllRevisions;

public class GetAllRevisionsManager : IGetAllRevisionsManager
{
    private readonly IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity> _currencyLifeCycleHandler;
    private readonly IEntityLifecycleHandler<DenominationEntity, DenominationRevisionEntity> _denominationLifeCycleHandler;
    private readonly ILogger<GetAllRevisionsManager> _logger;
    private readonly IMapper _mapper;

    public GetAllRevisionsManager(
        IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity> currencyLifeCycleHandler,
        IEntityLifecycleHandler<DenominationEntity, DenominationRevisionEntity> denominationLifeCycleHandler,
        ILogger<GetAllRevisionsManager> logger,
        IMapper mapper
        )
    {
        _currencyLifeCycleHandler = currencyLifeCycleHandler;
        _denominationLifeCycleHandler = denominationLifeCycleHandler;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ManagerResponse>> GetAllRevisionsAsync(Guid key, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting to retrieve all revisions for currency with key: {Key}", key);

        var currencyRevisions = await _currencyLifeCycleHandler.GetAllAsync(key, cancellationToken: cancellationToken);
        var denominationRevisions = await _denominationLifeCycleHandler.FindAllAsync(x => x.CurrencyKey == key, cancellationToken: cancellationToken);

        // Group all changes by timestamp
        var historicTimeSlices = currencyRevisions
            .Select(x => x.Revision.CreatedAt)
            .Concat(denominationRevisions.Select(x => x.Revision.CreatedAt))
            .Distinct()
            .OrderByDescending(x => x)
            .ToList();

        _logger.LogInformation("Retrieved {CurrencyRevisionsCount} currency revisions and {DenominationRevisionsCount} denomination revisions for key: {Key}", currencyRevisions.Count(), denominationRevisions.Count(), key);

        var compositeRevisions = new List<ManagerResponse>();

        foreach (var timeSlice in historicTimeSlices)
        {
            var historicCurrencyRevision = currencyRevisions.Where(x => x.Revision.CreatedAt <= timeSlice)
                .GroupBy(x => x.Key)
                .Select(x => x.OrderByDescending(y => y.Revision.Revision).First())
                .Select(x => _mapper.Map<EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity>>(x))
                .FirstOrDefault();
            var historicDenominationRevisions = denominationRevisions
                .Where(x => x.Revision.CreatedAt <= timeSlice)
                .OrderByDescending(x => x.Revision.Revision)
                .GroupBy(x => x.Key)
                .Select(x => x.OrderByDescending(y => y.Revision.Revision).First())
                .Select(x => _mapper.Map<EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>>(x))
                .ToList();

            if (historicCurrencyRevision != null)
            {
                compositeRevisions.Add(new ManagerResponse(historicCurrencyRevision, historicDenominationRevisions));
            }
        }

        var results = new List<ManagerResponse>();

        for (int i = 0; i < compositeRevisions.Count; i++)
        {
            var historicRevision = compositeRevisions[i];
            var historicDenominationRevisions = historicRevision.Denominations;
            var historicCurrencyRevision = historicRevision.Currency;

            if (historicCurrencyRevision == null || historicDenominationRevisions == null)
            {
                continue;
            }

            EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity>? previousCurrencyRevision = null;
            IEnumerable<EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>>? previousDenominationRevisions = null;

            if (i < compositeRevisions.Count - 1)
            {
                var previousHistoricRevision = compositeRevisions[i + 1];
                previousCurrencyRevision = previousHistoricRevision.Currency;
                previousDenominationRevisions = previousHistoricRevision.Denominations;
            }

            if (previousCurrencyRevision != null && previousCurrencyRevision.Revision.Revision == historicCurrencyRevision?.Revision.Revision)
            {
                historicCurrencyRevision = _mapper.Map<EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity>>(historicCurrencyRevision);
                historicCurrencyRevision.Revision.Action = RevisionAction.Unmodified;
            }

            var previousDeletedDenominations = previousDenominationRevisions?.Where(x => x.Revision.Action == RevisionAction.Deleted).ToList() ?? new List<EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>>();
            var resultingDenominations = historicDenominationRevisions?
                .Where(x => !previousDeletedDenominations.Any(y => y.Key == x.Key))
                .Select(x =>
                {
                    if (previousDenominationRevisions != null && previousDenominationRevisions.Any(y => y.Key == x.Key && y.Revision.Action == x.Revision.Action))
                    {
                        x.Revision.Action = RevisionAction.Unmodified;
                    }
                    return x;
                })
                .ToList();

            if (historicCurrencyRevision != null && resultingDenominations != null)
            {
                results.Add(new ManagerResponse(historicCurrencyRevision, resultingDenominations));
            }
        }

        _logger.LogInformation("Retrieved {ResultsCount} final revisions for key: {Key}", results.Count(), key);

        return results;
    }
}
