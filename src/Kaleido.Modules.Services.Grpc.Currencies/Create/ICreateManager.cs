using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Currencies.Create;

public interface ICreateManager
{
    Task<ManagerResponse> CreateAsync(CurrencyEntity createCurrency, CancellationToken cancellationToken = default);
}
