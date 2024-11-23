using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Grpc.Currencies;

namespace Kaleido.Modules.Services.Grpc.Currencies.Update;

public interface IUpdateHandler : IBaseHandler<CurrencyActionRequest, CurrencyResponse>;
