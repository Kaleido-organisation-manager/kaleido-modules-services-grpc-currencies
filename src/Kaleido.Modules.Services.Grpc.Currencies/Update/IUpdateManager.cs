using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Currencies.Update;

public interface IUpdateManager
{
    Task<ManagerResponse> UpdateAsync(Guid key, CurrencyEntity currency, IEnumerable<DenominationEntity> denominations, CancellationToken cancellationToken = default);
}
