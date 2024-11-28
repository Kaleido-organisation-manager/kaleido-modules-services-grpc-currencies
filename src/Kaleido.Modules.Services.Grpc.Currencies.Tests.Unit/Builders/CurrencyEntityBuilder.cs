using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.Builders;

public class CurrencyEntityBuilder
{
    private readonly CurrencyEntity _currency = new()
    {
        Name = "Euro",
        Code = "EUR",
        Symbol = "€",
    };

    public CurrencyEntityBuilder WithName(string name)
    {
        _currency.Name = name;
        return this;
    }

    public CurrencyEntityBuilder WithCode(string code)
    {
        _currency.Code = code;
        return this;
    }

    public CurrencyEntityBuilder WithSymbol(string symbol)
    {
        _currency.Symbol = symbol;
        return this;
    }

    public CurrencyEntity Build() => _currency;
}