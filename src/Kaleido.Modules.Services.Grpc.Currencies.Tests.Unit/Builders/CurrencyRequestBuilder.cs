using Kaleido.Grpc.Currencies;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.Builders;

public class CurrencyRequestBuilder
{
    private readonly CurrencyRequest _request = new()
    {
        Key = Guid.NewGuid().ToString(),
    };

    public CurrencyRequestBuilder WithKey(string key)
    {
        _request.Key = key;
        return this;
    }

    public CurrencyRequest Build() => _request;
}
