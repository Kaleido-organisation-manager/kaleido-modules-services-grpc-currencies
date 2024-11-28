using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.Builders;

public class DenominationEntityBuilder
{
    private readonly DenominationEntity _denomination = new()
    {
        Value = 1.00M,
        Description = "1 euro",
        CurrencyKey = Guid.NewGuid()
    };

    public DenominationEntityBuilder WithValue(decimal value)
    {
        _denomination.Value = value;
        return this;
    }

    public DenominationEntityBuilder WithDescription(string description)
    {
        _denomination.Description = description;
        return this;
    }

    public DenominationEntityBuilder WithCurrencyKey(Guid currencyKey)
    {
        _denomination.CurrencyKey = currencyKey;
        return this;
    }

    public DenominationEntity Build() => _denomination;
}