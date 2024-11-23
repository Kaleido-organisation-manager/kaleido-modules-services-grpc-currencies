using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Currencies.GetAll;

public interface IGetAllManager
{
    Task<IEnumerable<ManagerResponse>> GetAllAsync(CancellationToken cancellationToken = default);
}
