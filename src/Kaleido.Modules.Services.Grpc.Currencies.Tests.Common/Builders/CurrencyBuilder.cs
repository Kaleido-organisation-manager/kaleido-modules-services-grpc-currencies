using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Common.Builders;

public class CurrencyBuilder
{
    private readonly Currency _currency = new()
    {
        Code = "EUR",
        Name = "Euro",
        Symbol = "€",
        Denominations = { new List<Denomination> {
                new DenominationBuilder().Build(),
            },
        },
    };

    public CurrencyBuilder WithName(string name)
    {
        _currency.Name = name;
        return this;
    }

    public CurrencyBuilder WithCode(string code)
    {
        _currency.Code = code;
        return this;
    }

    public CurrencyBuilder WithSymbol(string symbol)
    {
        _currency.Symbol = symbol;
        return this;
    }

    public CurrencyBuilder ForCurrencyEntity(CurrencyEntity entity)
    {
        _currency.Name = entity.Name;
        _currency.Code = entity.Code;
        _currency.Symbol = entity.Symbol;
        return this;
    }

    public CurrencyBuilder WithDenominations(List<Denomination> denominations)
    {
        _currency.Denominations.Clear();
        _currency.Denominations.AddRange(denominations);
        return this;
    }

    public Currency Build()
    {
        return _currency;
    }
}
