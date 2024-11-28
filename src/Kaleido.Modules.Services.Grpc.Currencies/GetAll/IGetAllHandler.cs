using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Grpc.Currencies;

namespace Kaleido.Modules.Services.Grpc.Currencies.GetAll;

public interface IGetAllHandler : IBaseHandler<EmptyRequest, CurrencyListResponse>;