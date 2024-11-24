using Kaleido.Grpc.Currencies;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Common.Builders;

public class DenominationBuilder
{
    private readonly Denomination _denomination = new()
    {
        Value = 1.00F,
        Description = "1 euro",
    };

    public DenominationBuilder WithValue(decimal value)
    {
        _denomination.Value = (float)value;
        return this;
    }

    public DenominationBuilder WithDescription(string description)
    {
        _denomination.Description = description;
        return this;
    }

    public Denomination Build()
    {
        return _denomination;
    }
}
