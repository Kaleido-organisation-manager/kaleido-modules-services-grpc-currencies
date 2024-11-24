using AutoMapper;
using FluentValidation;
using Grpc.Core;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Validators;

namespace Kaleido.Modules.Services.Grpc.Currencies.Create;

public class CreateHandler : ICreateHandler
{
    private readonly ICreateManager _createManager;
    private readonly ILogger<CreateHandler> _logger;
    private readonly IMapper _mapper;
    public readonly CurrencyValidator _validator;

    public CreateHandler(
        ICreateManager createManager,
        ILogger<CreateHandler> logger,
        IMapper mapper,
        CurrencyValidator validator
        )
    {
        _createManager = createManager;
        _logger = logger;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<CurrencyResponse> HandleAsync(Currency request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling CreateCurrency request for name: {Name}", request.Name);

        try
        {
            await _validator.ValidateAndThrowAsync(request, cancellationToken);
            var currency = _mapper.Map<CurrencyEntity>(request);
            var denominations = _mapper.Map<IEnumerable<DenominationEntity>>(request.Denominations.ToList());
            var result = await _createManager.CreateAsync(currency, denominations, cancellationToken);

            return result.ToCurrencyResponse(_mapper);
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Validation failed for currency with name: {Name}. Errors: {Errors}", request.Name, ex.Errors.Select(e => e.ErrorMessage));
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message, ex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured while creating currency with name: {Name}", request.Name);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message, ex));
        }
    }
}