using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Common.Builders;

public class CurrencyEntityBuilder
{
    private readonly CurrencyEntity _currencyEntity = new()
    {
        Id = Guid.NewGuid(),
        Code = "EUR",
        Name = "Euro",
        Symbol = "€",
    };

    public CurrencyEntityBuilder FromCurrency(Currency currency)
    {
        _currencyEntity.Name = currency.Name;
        _currencyEntity.Code = currency.Code;
        _currencyEntity.Symbol = currency.Symbol;
        return this;
    }

    public CurrencyEntityBuilder WithName(string name)
    {
        _currencyEntity.Name = name;
        return this;
    }

    public CurrencyEntityBuilder WithCode(string code)
    {
        _currencyEntity.Code = code;
        return this;
    }

    public CurrencyEntityBuilder WithSymbol(string symbol)
    {
        _currencyEntity.Symbol = symbol;
        return this;
    }

    public CurrencyEntity Build()
    {
        return _currencyEntity;
    }
}
