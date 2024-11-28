using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Grpc.Currencies;

namespace Kaleido.Modules.Services.Grpc.Currencies.Delete;

public interface IDeleteHandler : IBaseHandler<CurrencyRequest, CurrencyResponse>;