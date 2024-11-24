using Kaleido.Common.Services.Grpc.Models;

namespace Kaleido.Modules.Services.Grpc.Currencies.Common.Models;

public class DenominationEntity : BaseEntity
{
    public Guid CurrencyKey { get; set; }
    public decimal Value { get; set; }
    public string? Description { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is DenominationEntity entity &&
               CurrencyKey == entity.CurrencyKey &&
               Value == entity.Value &&
               Description == entity.Description;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), CurrencyKey, Value, Description);
    }
}
