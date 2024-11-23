using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Currencies.GetAll;

public class GetAllManager : IGetAllManager
{
    private readonly IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity> _currencyLifeCycleHandler;
    private readonly ILogger<GetAllManager> _logger;

    public GetAllManager(
        IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity> repository,
         ILogger<GetAllManager> logger
         )
    {
        _currencyLifeCycleHandler = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<ManagerResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all active currencies");
        var currencies = await _currencyLifeCycleHandler.FindAllAsync(
            c => true,
            r => r.Action != RevisionAction.Deleted && r.Status == RevisionStatus.Active,
            cancellationToken: cancellationToken);
        return currencies.Select(c => ManagerResponse.Success(c));
    }
}
