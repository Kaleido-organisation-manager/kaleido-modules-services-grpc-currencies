using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Grpc.Currencies;

namespace Kaleido.Modules.Services.Grpc.Currencies.Create;

public interface ICreateHandler : IBaseHandler<Currency, CurrencyResponse>;