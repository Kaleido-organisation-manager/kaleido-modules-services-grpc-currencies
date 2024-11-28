using Kaleido.Grpc.Currencies;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Integrations.Builders;

public class CurrencyActionRequestBuilder
{
    private readonly CurrencyActionRequest _request = new()
    {
        Key = Guid.NewGuid().ToString(),
        Currency = new CurrencyBuilder().Build(),
    };

    public CurrencyActionRequestBuilder WithKey(string key)
    {
        _request.Key = key;
        return this;
    }

    public CurrencyActionRequestBuilder WithCurrency(Currency currency)
    {
        _request.Currency = currency;
        return this;
    }

    public CurrencyActionRequest Build() => _request;
}
