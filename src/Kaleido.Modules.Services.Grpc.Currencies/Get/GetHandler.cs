using AutoMapper;
using FluentValidation;
using Grpc.Core;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Constants;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Validators;

namespace Kaleido.Modules.Services.Grpc.Currencies.Get;

public class GetHandler : IGetHandler
{
    private readonly IGetManager _manager;
    private readonly ILogger<GetHandler> _logger;
    public readonly KeyValidator _validator;
    public readonly IMapper _mapper;

    public GetHandler(
        IGetManager manager,
        KeyValidator validator,
        ILogger<GetHandler> logger,
        IMapper mapper
    )
    {
        _manager = manager;
        _validator = validator;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<CurrencyResponse> HandleAsync(CurrencyRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling GetCurrency request for key: {Key}", request.Key);

        ManagerResponse result;

        try
        {
            await _validator.ValidateAndThrowAsync(request.Key, cancellationToken);
            result = await _manager.GetAsync(request.Key, cancellationToken);
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Validation failed for get category. Key: {Key}. Errors: {Errors}", request.Key, ex.Errors.Select(e => e.ErrorMessage));
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message, ex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category with key: {Key}", request.Key);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message, ex));
        }

        if (result.State == ManagerResponseState.NotFound)
        {
            _logger.LogWarning("Currency with key {Key} not found", request.Key);
            throw new RpcException(new Status(StatusCode.NotFound, "Category not found"));
        }

        return _mapper.Map<CurrencyResponse>(result.Currency);
    }
}
