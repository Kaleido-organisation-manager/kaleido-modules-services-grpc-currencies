using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Currencies.Delete;

public interface IDeleteManager
{
    Task<ManagerResponse> DeleteAsync(string key, CancellationToken cancellationToken = default);
}
