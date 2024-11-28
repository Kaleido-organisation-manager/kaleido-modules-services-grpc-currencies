using AutoMapper;
using FluentValidation;
using Grpc.Core;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Constants;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Validators;

namespace Kaleido.Modules.Services.Grpc.Currencies.Update;

public class UpdateHandler : IUpdateHandler
{
    private readonly IUpdateManager _updateManager;
    private readonly ILogger<UpdateHandler> _logger;
    private readonly KeyValidator _keyValidator;
    private readonly CurrencyValidator _currencyValidator;
    private readonly IMapper _mapper;

    public UpdateHandler(
        IUpdateManager updateManager,
        ILogger<UpdateHandler> logger,
        KeyValidator keyValidator,
        CurrencyValidator currencyValidator,
        IMapper mapper
    )
    {
        _updateManager = updateManager;
        _logger = logger;
        _keyValidator = keyValidator;
        _currencyValidator = currencyValidator;
        _mapper = mapper;
    }

    public async Task<CurrencyResponse> HandleAsync(CurrencyActionRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling UpdateCurrency request with key: {Key}", request.Key);

        ManagerResponse updateResult;

        try
        {
            await _keyValidator.ValidateAndThrowAsync(request.Key, cancellationToken);
            var key = Guid.Parse(request.Key);
            await _currencyValidator.ValidateAndThrowAsync(request.Currency, cancellationToken);
            var currency = _mapper.Map<CurrencyEntity>(request.Currency);
            var denominations = _mapper.Map<IEnumerable<DenominationEntity>>(request.Currency.Denominations);
            denominations.ToList().ForEach(d => d.CurrencyKey = key);
            updateResult = await _updateManager.UpdateAsync(key, currency, denominations, cancellationToken);
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Validation failed for update currency. Key: {Key}. Errors: {Errors}", request.Key, ex.Errors.Select(e => e.ErrorMessage));
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message, ex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating currency");
            throw new RpcException(new Status(StatusCode.Internal, ex.Message, ex));
        }

        if (updateResult.State == ManagerResponseState.NotFound)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Currency with key {request.Key} not found"));
        }

        return updateResult.ToCurrencyResponse(_mapper);
    }
}
