using AutoMapper;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Constants;

namespace Kaleido.Modules.Services.Grpc.Currencies.Common.Models;

public readonly struct ManagerResponse
{
    public readonly ManagerResponseState State = ManagerResponseState.Success;
    public readonly EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity>? Currency;
    public readonly IEnumerable<EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>>? Denominations;

    public ManagerResponse(EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity> currency, IEnumerable<EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>> denominations)
    {
        Currency = currency;
        Denominations = denominations;
        State = ManagerResponseState.Success;
    }

    public ManagerResponse(ManagerResponseState state)
    {
        State = state;
        Currency = null;
        Denominations = null;
    }

    public CurrencyResponse ToCurrencyResponse(IMapper mapper)
    {
        var currencyResult = mapper.Map<EntityLifeCycleResult<CurrencyWithDenominations, BaseRevisionEntity>>(Currency);
        currencyResult.Entity.Denominations = Denominations ?? [];

        var response = mapper.Map<CurrencyResponse>(currencyResult);
        return response;
    }

    public static ManagerResponse NotFound() => new(ManagerResponseState.NotFound);

    public static ManagerResponse Success(EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity> currency, IEnumerable<EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>> denominations) => new(currency, denominations);
}
