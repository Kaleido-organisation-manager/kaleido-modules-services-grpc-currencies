using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Grpc.Currencies;

namespace Kaleido.Modules.Services.Grpc.Currencies.GetRevision;

public interface IGetRevisionHandler : IBaseHandler<GetCurrencyRevisionRequest, CurrencyResponse>;