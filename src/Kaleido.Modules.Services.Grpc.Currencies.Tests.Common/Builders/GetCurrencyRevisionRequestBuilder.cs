using Google.Protobuf.WellKnownTypes;
using Kaleido.Grpc.Currencies;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Common.Builders;

public class GetCurrencyRevisionRequestBuilder
{
    private readonly GetCurrencyRevisionRequest _request = new()
    {
        Key = Guid.NewGuid().ToString(),
        CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow),
    };

    public GetCurrencyRevisionRequestBuilder WithKey(string key)
    {
        _request.Key = key;
        return this;
    }

    public GetCurrencyRevisionRequestBuilder WithCreatedAt(DateTime createdAt)
    {
        _request.CreatedAt = Timestamp.FromDateTime(createdAt);
        return this;
    }

    public GetCurrencyRevisionRequest Build() => _request;
}
