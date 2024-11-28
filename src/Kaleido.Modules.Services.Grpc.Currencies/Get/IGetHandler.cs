using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Grpc.Currencies;

namespace Kaleido.Modules.Services.Grpc.Currencies.Get;

public interface IGetHandler : IBaseHandler<CurrencyRequest, CurrencyResponse>;
