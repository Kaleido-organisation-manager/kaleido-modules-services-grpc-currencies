using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Currencies.GetAllFiltered;

public interface IGetAllFilteredManager
{
    Task<IEnumerable<ManagerResponse>> GetAllFilteredAsync(string name, CancellationToken cancellationToken = default);
}
