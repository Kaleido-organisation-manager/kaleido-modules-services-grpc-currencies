using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Currencies.Get;

public interface IGetManager
{
    Task<ManagerResponse> GetAsync(string key, CancellationToken cancellationToken = default);
}
