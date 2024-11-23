namespace Kaleido.Modules.Services.Grpc.Currencies.Common.Models;
using Kaleido.Common.Services.Grpc.Models;

public class CurrencyEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Symbol { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is CurrencyEntity other &&
            other.Name == Name &&
            other.Code == Code &&
            other.Symbol == Symbol;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Name, Code, Symbol);
    }
}