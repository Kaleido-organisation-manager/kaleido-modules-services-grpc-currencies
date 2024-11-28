using Grpc.Core;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Integrations.Builders;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Integrations.Fixtures;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Integrations.Get;

[Collection("Infrastructure collection")]
public class GetIntegrationTests
{
    private readonly InfrastructureFixture _fixture;

    public GetIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearDatabase().Wait();
    }

    [Fact]
    public async Task GetAsync_ShouldReturnCurrency()
    {
        // Arrange
        var currency = new CurrencyBuilder()
            .WithName("Test Currency")
            .WithCode("TST")
            .WithSymbol("T")
            .WithDenominations(new List<Denomination>
            {
                new DenominationBuilder()
                    .WithValue(1.00M)
                    .WithDescription("Test Denomination")
                    .Build()
            })
            .Build();

        var createResponse = await _fixture.Client.CreateCurrencyAsync(currency);
        var request = new CurrencyRequestBuilder()
            .WithKey(createResponse.Revision.Key)
            .Build();

        // Act
        var getResponse = await _fixture.Client.GetCurrencyAsync(request);

        // Assert
        Assert.NotNull(getResponse);
        Assert.Equal(createResponse.Revision.Key, getResponse.Revision.Key);
        Assert.Equal("Test Currency", getResponse.Currency.Name);
        Assert.Equal("TST", getResponse.Currency.Code);
        Assert.Equal("T", getResponse.Currency.Symbol);
        Assert.Single(getResponse.Currency.Denominations);
        Assert.Equal(1.00f, getResponse.Currency.Denominations[0].Denomination.Value);
        Assert.Equal("Test Denomination", getResponse.Currency.Denominations[0].Denomination.Description);
    }

    [Fact]
    public async Task GetAsync_WithMultipleDenominations_ShouldReturnAllDenominations()
    {
        // Arrange
        var currency = new CurrencyBuilder()
            .WithName("Test Currency")
            .WithDenominations(new List<Denomination>
            {
                new DenominationBuilder().WithValue(1.00M).WithDescription("One").Build(),
                new DenominationBuilder().WithValue(2.00M).WithDescription("Two").Build()
            })
            .Build();

        var createResponse = await _fixture.Client.CreateCurrencyAsync(currency);
        var request = new CurrencyRequestBuilder()
            .WithKey(createResponse.Revision.Key)
            .Build();

        // Act
        var getResponse = await _fixture.Client.GetCurrencyAsync(request);

        // Assert
        Assert.NotNull(getResponse);
        Assert.Equal(2, getResponse.Currency.Denominations.Count);
        Assert.Contains(getResponse.Currency.Denominations,
            d => d.Denomination.Value == 1.00f && d.Denomination.Description == "One");
        Assert.Contains(getResponse.Currency.Denominations,
            d => d.Denomination.Value == 2.00f && d.Denomination.Description == "Two");
    }

    [Fact]
    public async Task GetAsync_DeletedCurrency_ShouldThrowNotFound()
    {
        // Arrange
        var currency = new CurrencyBuilder()
            .WithName("Test Currency")
            .WithDenominations(new List<Denomination>
            {
                new DenominationBuilder().WithValue(1.00M).Build()
            })
            .Build();

        var createResponse = await _fixture.Client.CreateCurrencyAsync(currency);
        var request = new CurrencyRequestBuilder()
            .WithKey(createResponse.Revision.Key)
            .Build();

        await _fixture.Client.DeleteCurrencyAsync(request);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.GetCurrencyAsync(request));
        Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
    }

    [Fact]
    public async Task GetAsync_NonExistentCurrency_ShouldThrowNotFound()
    {
        // Arrange
        var request = new CurrencyRequestBuilder()
            .WithKey(Guid.NewGuid().ToString())
            .Build();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.GetCurrencyAsync(request));
        Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid-guid")]
    public async Task GetAsync_InvalidKey_ShouldThrowInvalidArgument(string key)
    {
        // Arrange
        var request = new CurrencyRequestBuilder()
            .WithKey(key)
            .Build();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.GetCurrencyAsync(request));
        Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
    }
}