using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Constants;

namespace Kaleido.Modules.Services.Grpc.Currencies.Common.Models;

public readonly struct ManagerResponse
{
    public readonly ManagerResponseState State = ManagerResponseState.Success;
    public readonly EntityLifeCycleResult<CurrencyEntity, BaseRevisionEntity>? Currency;

    public ManagerResponse(EntityLifeCycleResult<CurrencyEntity, BaseRevisionEntity> currency)
    {
        Currency = currency;
        State = ManagerResponseState.Success;
    }

    public ManagerResponse(ManagerResponseState state)
    {
        State = state;
        Currency = null;
    }

    public static ManagerResponse NotFound() => new(ManagerResponseState.NotFound);

    public static ManagerResponse Success(EntityLifeCycleResult<CurrencyEntity, BaseRevisionEntity> currency) => new(currency);
}
