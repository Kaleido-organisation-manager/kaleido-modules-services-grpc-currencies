using AutoMapper;
using Grpc.Core;
using Kaleido.Grpc.Currencies;

namespace Kaleido.Modules.Services.Grpc.Currencies.GetAll;

public class GetAllHandler : IGetAllHandler
{
    private readonly IGetAllManager _manager;
    private readonly ILogger<GetAllHandler> _logger;
    private readonly IMapper _mapper;

    public GetAllHandler(
        IGetAllManager manager,
        ILogger<GetAllHandler> logger,
        IMapper mapper
    )
    {
        _manager = manager;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<CurrencyListResponse> HandleAsync(EmptyRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling GetAllCurrencies request");

        try
        {
            var result = await _manager.GetAllAsync(cancellationToken);
            return _mapper.Map<CurrencyListResponse>(result.Select(r => r.Currency));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling GetAllCurrencies request");
            throw new RpcException(new Status(StatusCode.Internal, ex.Message, ex));
        }
    }
}
