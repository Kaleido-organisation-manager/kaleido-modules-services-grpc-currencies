using AutoMapper;
using FluentValidation;
using Grpc.Core;
using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Constants;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Validators;

namespace Kaleido.Modules.Services.Grpc.Currencies.Delete;

public class DeleteHandler : IDeleteHandler
{
    private readonly IDeleteManager _deleteManager;
    private readonly ILogger<DeleteHandler> _logger;
    private readonly KeyValidator _validator;
    private readonly IMapper _mapper;

    public DeleteHandler(
        IDeleteManager deleteManager,
        ILogger<DeleteHandler> logger,
        KeyValidator validator,
        IMapper mapper
        )
    {
        _deleteManager = deleteManager;
        _logger = logger;
        _validator = validator;
        _mapper = mapper;
    }


    public async Task<CurrencyResponse> HandleAsync(CurrencyRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling DeleteCurrency request for key: {Key}", request.Key);

        ManagerResponse result;
        try
        {
            await _validator.ValidateAndThrowAsync(request.Key, cancellationToken);
            var key = Guid.Parse(request.Key);
            result = await _deleteManager.DeleteAsync(key, cancellationToken);
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Validation failed for category deletion. Key: {Key}. Errors: {Errors}", request.Key, ex.Errors.Select(e => e.ErrorMessage));
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message, ex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured while deleting category with key: {Key}", request.Key);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message, ex));
        }

        if (result.Currency == null || result.State == ManagerResponseState.NotFound)
        {
            _logger.LogWarning("Currency with key {Key} not found", request.Key);
            throw new RpcException(new Status(StatusCode.NotFound, $"Could not find currency with key {request.Key}"));
        }

        return result.ToCurrencyResponse(_mapper);
    }
}
