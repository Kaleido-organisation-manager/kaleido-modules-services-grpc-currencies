using AutoMapper;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Currencies.Get;

public class GetManager : IGetManager
{
    private readonly IMapper _mapper;
    private readonly IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity> _currencyLifeCycleHandler;
    private readonly ILogger<GetManager> _logger;

    public GetManager(
        IMapper mapper,
        IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity> currencyLifeCycleHandler,
        ILogger<GetManager> logger
    )
    {
        _mapper = mapper;
        _currencyLifeCycleHandler = currencyLifeCycleHandler;
        _logger = logger;
    }

    public async Task<ManagerResponse> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting currency with key: {Key}", key);
        var result = await _currencyLifeCycleHandler.GetAsync(Guid.Parse(key), cancellationToken: cancellationToken);

        if (result == null || result.Revision.Action == RevisionAction.Deleted)
        {
            return ManagerResponse.NotFound();
        }
        return ManagerResponse.Success(result);
    }
}
