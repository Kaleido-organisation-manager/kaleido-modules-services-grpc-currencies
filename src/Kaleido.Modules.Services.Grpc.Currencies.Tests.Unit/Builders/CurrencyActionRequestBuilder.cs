using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.Builders;

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

    public CurrencyActionRequestBuilder ForCurrencyEntity(Guid key, CurrencyEntity entity)
    {
        _request.Key = key.ToString();
        _request.Currency = new CurrencyBuilder().ForCurrencyEntity(entity).Build();
        return this;
    }

    public CurrencyActionRequest Build() => _request;
}
