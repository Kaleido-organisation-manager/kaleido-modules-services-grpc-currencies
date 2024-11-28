using Kaleido.Grpc.Currencies;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Integrations.Builders;

public class GetAllCurrenciesFilteredRequestBuilder
{
    private readonly GetAllCurrenciesFilteredRequest _request = new()
    {
        Name = "Euro",
    };

    public GetAllCurrenciesFilteredRequestBuilder WithName(string name)
    {
        _request.Name = name;
        return this;
    }

    public GetAllCurrenciesFilteredRequest Build() => _request;
}
