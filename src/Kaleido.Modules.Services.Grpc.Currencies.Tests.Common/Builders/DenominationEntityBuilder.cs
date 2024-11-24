using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Common.Builders;

public class DenominationEntityBuilder
{
    private readonly DenominationEntity _denominationEntity = new()
    {
        Id = Guid.NewGuid(),
        CurrencyKey = Guid.NewGuid(),
        Value = 1.00M,
        Description = "1 euro",
    };

    public DenominationEntityBuilder WithCurrencyKey(Guid currencyKey)
    {
        _denominationEntity.CurrencyKey = currencyKey;
        return this;
    }

    public DenominationEntityBuilder WithValue(decimal value)
    {
        _denominationEntity.Value = value;
        return this;
    }

    public DenominationEntityBuilder WithDescription(string description)
    {
        _denominationEntity.Description = description;
        return this;
    }

    public DenominationEntity Build()
    {
        return _denominationEntity;
    }
}
