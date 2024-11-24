using Kaleido.Common.Services.Grpc.Models;

namespace Kaleido.Modules.Services.Grpc.Currencies.Common.Models;

public class CurrencyWithDenominations : CurrencyEntity
{
    public required IEnumerable<EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>> Denominations { get; set; }
}
