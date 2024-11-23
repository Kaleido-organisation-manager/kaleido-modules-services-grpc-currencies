using AutoMapper;
using FluentValidation;
using Grpc.Core;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Validators;

namespace Kaleido.Modules.Services.Grpc.Currencies.GetAllFiltered;

public class GetAllFilteredHandler : IGetAllFilteredHandler
{
    private readonly IGetAllFilteredManager _manager;
    private readonly ILogger<GetAllFilteredHandler> _logger;
    private readonly NameValidator _validator;
    private readonly IMapper _mapper;

    public GetAllFilteredHandler(
        IGetAllFilteredManager manager,
        ILogger<GetAllFilteredHandler> logger,
        NameValidator validator,
        IMapper mapper
        )
    {
        _manager = manager;
        _logger = logger;
        _validator = validator;
        _mapper = mapper;
    }

    public async Task<CurrencyListResponse> HandleAsync(GetAllCurrenciesFilteredRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling GetAllCurrenciesFiltered request for name: {name}", request.Name);

        try
        {
            _validator.ValidateAndThrow(request.Name);
            var currencies = await _manager.GetAllByNameAsync(request.Name, cancellationToken);
            return _mapper.Map<CurrencyListResponse>(currencies.Select(c => c.Currency));
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Validation failed for get all currency by name. Name: {Name}. Errors: {Errors}", request.Name, ex.Errors.Select(e => e.ErrorMessage));
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message, ex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all currencies by name: {name}", request.Name);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message, ex));
        }
    }
}
