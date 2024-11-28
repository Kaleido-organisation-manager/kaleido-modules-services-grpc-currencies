using Grpc.Core;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.Create;
using Kaleido.Modules.Services.Grpc.Currencies.Delete;
using Kaleido.Modules.Services.Grpc.Currencies.Get;
using Kaleido.Modules.Services.Grpc.Currencies.GetAll;
using Kaleido.Modules.Services.Grpc.Currencies.GetAllFiltered;
using Kaleido.Modules.Services.Grpc.Currencies.GetAllRevisions;
using Kaleido.Modules.Services.Grpc.Currencies.GetRevision;
using Kaleido.Modules.Services.Grpc.Currencies.Update;

namespace Kaleido.Modules.Services.Grpc.Currencies.Common.Services;

public class CurrencyService : GrpcCurrencies.GrpcCurrenciesBase
{
    private readonly ICreateHandler _createHandler;
    private readonly IDeleteHandler _deleteHandler;
    private readonly IGetHandler _getHandler;
    private readonly IGetAllHandler _getAllHandler;
    private readonly IGetAllFilteredHandler _getAllFilteredHandler;
    private readonly IGetAllRevisionsHandler _getAllRevisionsHandler;
    private readonly IGetRevisionHandler _getRevisionHandler;
    private readonly IUpdateHandler _updateHandler;

    private readonly ILogger<CurrencyService> _logger;

    public CurrencyService(
        ICreateHandler createHandler,
        IDeleteHandler deleteHandler,
        IGetHandler getHandler,
        IGetAllHandler getAllHandler,
        IGetAllFilteredHandler getAllFilteredHandler,
        IGetAllRevisionsHandler getAllRevisionsHandler,
        IGetRevisionHandler getRevisionHandler,
        IUpdateHandler updateHandler,
        ILogger<CurrencyService> logger)
    {
        _createHandler = createHandler;
        _deleteHandler = deleteHandler;
        _getHandler = getHandler;
        _getAllHandler = getAllHandler;
        _getAllFilteredHandler = getAllFilteredHandler;
        _getAllRevisionsHandler = getAllRevisionsHandler;
        _getRevisionHandler = getRevisionHandler;
        _updateHandler = updateHandler;
        _logger = logger;
    }

    public override async Task<CurrencyResponse> CreateCurrency(Currency request, ServerCallContext context)
    {
        _logger.LogInformation("Creating currency: {Name}", request.Name);
        return await _createHandler.HandleAsync(request, context.CancellationToken);
    }

    public override async Task<CurrencyResponse> DeleteCurrency(CurrencyRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Deleting currency: {Key}", request.Key);
        return await _deleteHandler.HandleAsync(request, context.CancellationToken);
    }

    public override async Task<CurrencyResponse> GetCurrency(CurrencyRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Getting currency: {Key}", request.Key);
        return await _getHandler.HandleAsync(request, context.CancellationToken);
    }

    public override async Task<CurrencyListResponse> GetAllCurrencies(EmptyRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Getting all currencies");
        return await _getAllHandler.HandleAsync(request, context.CancellationToken);
    }

    public override async Task<CurrencyListResponse> GetAllCurrenciesFiltered(
        GetAllCurrenciesFilteredRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("Getting all currencies filtered by name: {Name}", request.Name);
        return await _getAllFilteredHandler.HandleAsync(request, context.CancellationToken);
    }

    public override async Task<CurrencyListResponse> GetAllCurrencyRevisions(CurrencyRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Getting all currency revisions for key: {Key}", request.Key);
        return await _getAllRevisionsHandler.HandleAsync(request, context.CancellationToken);
    }

    public override async Task<CurrencyResponse> GetCurrencyRevision(GetCurrencyRevisionRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Getting currency revision for key: {Key} and createdAt: {CreatedAt}", request.Key, request.CreatedAt);
        return await _getRevisionHandler.HandleAsync(request, context.CancellationToken);
    }

    public override async Task<CurrencyResponse> UpdateCurrency(CurrencyActionRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Updating currency: {Key}", request.Key);
        return await _updateHandler.HandleAsync(request, context.CancellationToken);
    }
}
