using AutoMapper;
using FluentValidation;
using Grpc.Core;
using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Validators;

namespace Kaleido.Modules.Services.Grpc.Currencies.GetAllRevisions;

public class GetAllRevisionsHandler : IGetAllRevisionsHandler
{
    private readonly IGetAllRevisionsManager _manager;
    private readonly ILogger<GetAllRevisionsHandler> _logger;
    private readonly KeyValidator _validator;
    private readonly IMapper _mapper;

    public GetAllRevisionsHandler(
        IGetAllRevisionsManager manager,
        ILogger<GetAllRevisionsHandler> logger,
        KeyValidator validator,
        IMapper mapper
        )
    {
        _manager = manager;
        _logger = logger;
        _validator = validator;
        _mapper = mapper;
    }

    public async Task<CurrencyListResponse> HandleAsync(CurrencyRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling GetAllRevisions request for currency with key: {Key}", request.Key);

        try
        {
            await _validator.ValidateAndThrowAsync(request.Key, cancellationToken);
            var key = Guid.Parse(request.Key);
            var revisions = await _manager.GetAllRevisionsAsync(key, cancellationToken);
            return _mapper.Map<CurrencyListResponse>(revisions.Select(r => r.Currency));
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Validation failed for get all currency revisions. Key: {Key}. Errors: {Errors}", request.Key, ex.Errors.Select(e => e.ErrorMessage));
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message, ex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling GetAllRevisions request for currency with key: {Key}", request.Key);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message, ex));
        }
    }
}
