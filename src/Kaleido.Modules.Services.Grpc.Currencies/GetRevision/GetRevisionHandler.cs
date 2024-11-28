using AutoMapper;
using FluentValidation;
using Grpc.Core;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Constants;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Validators;

namespace Kaleido.Modules.Services.Grpc.Currencies.GetRevision;

public class GetRevisionHandler : IGetRevisionHandler
{
    private readonly IGetRevisionManager _manager;
    private readonly ILogger<GetRevisionHandler> _logger;
    private readonly KeyValidator _keyValidator;
    private readonly IMapper _mapper;

    public GetRevisionHandler(
        IGetRevisionManager manager,
        ILogger<GetRevisionHandler> logger,
        KeyValidator keyValidator,
        IMapper mapper
    )
    {
        _manager = manager;
        _logger = logger;
        _keyValidator = keyValidator;
        _mapper = mapper;
    }

    public async Task<CurrencyResponse> HandleAsync(GetCurrencyRevisionRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling GetCurrencyRevision request for currency with key: {Key} and created at: {CreatedAt}", request.Key, request.CreatedAt.ToDateTime());

        ManagerResponse result;

        try
        {
            await _keyValidator.ValidateAndThrowAsync(request.Key, cancellationToken);
            var key = Guid.Parse(request.Key);
            result = await _manager.GetRevisionAsync(key, request.CreatedAt.ToDateTime(), cancellationToken);
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Validation failed for get category revision. Key: {Key}. CreatedAt {CreatedAt}. Errors: {Errors}", request.Key, request.CreatedAt.ToDateTime(), ex.Errors.Select(e => e.ErrorMessage));
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message, ex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category revision");
            throw new RpcException(new Status(StatusCode.Internal, ex.Message, ex));
        }

        if (result.State == ManagerResponseState.NotFound)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Currency revision not found"));
        }

        return result.ToCurrencyResponse(_mapper);
    }
}
