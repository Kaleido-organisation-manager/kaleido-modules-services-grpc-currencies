using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Currencies.GetAllFiltered;

public class GetAllFilteredManager : IGetAllFilteredManager
{
    private readonly IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity> _lifeCycleHandler;
    private readonly ILogger<GetAllFilteredManager> _logger;

    public GetAllFilteredManager(
        IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity> repository,
        ILogger<GetAllFilteredManager> logger
        )
    {
        _lifeCycleHandler = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<ManagerResponse>> GetAllByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all currencies by name: {name}", name);
        var matchingCurrencies = await _lifeCycleHandler.FindAllAsync(
            (x) => x.Name.ToLower().Contains(name.ToLower()),
            (x) => x.Action != RevisionAction.Deleted && x.Status == RevisionStatus.Active,
            cancellationToken: cancellationToken);
        return matchingCurrencies.Select(c => ManagerResponse.Success(c));
    }
}
