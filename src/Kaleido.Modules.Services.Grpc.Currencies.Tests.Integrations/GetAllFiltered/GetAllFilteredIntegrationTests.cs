using Grpc.Core;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Integrations.Builders;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Integrations.Fixtures;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Integrations.GetAllFiltered;

[Collection("Infrastructure collection")]
public class GetAllFilteredIntegrationTests
{
    private readonly InfrastructureFixture _fixture;

    public GetAllFilteredIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearDatabase().Wait();
    }

    [Fact]
    public async Task GetAllFilteredAsync_WithName_ShouldReturnMatchingCurrencies()
    {
        // Arrange
        var currency1 = new CurrencyBuilder()
            .WithName("Euro Currency")
            .WithCode("EUR")
            .WithDenominations(new List<Denomination>
            {
                new DenominationBuilder().WithValue(1.00M).Build()
            })
            .Build();

        var currency2 = new CurrencyBuilder()
            .WithName("Dollar Currency")
            .WithCode("USD")
            .WithDenominations(new List<Denomination>
            {
                new DenominationBuilder().WithValue(1.00M).Build()
            })
            .Build();

        await _fixture.Client.CreateCurrencyAsync(currency1);
        await _fixture.Client.CreateCurrencyAsync(currency2);

        // Act
        var response = await _fixture.Client.GetAllCurrenciesFilteredAsync(
            new GetAllCurrenciesFilteredRequest { Name = "Euro" });

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.Currencies);
        Assert.Contains(response.Currencies, c => c.Currency.Name == "Euro Currency");
    }

    [Fact]
    public async Task GetAllFilteredAsync_WithPartialName_ShouldReturnAllMatches()
    {
        // Arrange
        var currency1 = new CurrencyBuilder()
            .WithName("Test Currency One")
            .WithDenominations(new List<Denomination>
            {
                new DenominationBuilder().WithValue(1.00M).Build()
            })
            .Build();

        var currency2 = new CurrencyBuilder()
            .WithName("Test Currency Two")
            .WithDenominations(new List<Denomination>
            {
                new DenominationBuilder().WithValue(1.00M).Build()
            })
            .Build();

        var currency3 = new CurrencyBuilder()
            .WithName("Different Name")
            .WithDenominations(new List<Denomination>
            {
                new DenominationBuilder().WithValue(1.00M).Build()
            })
            .Build();

        await _fixture.Client.CreateCurrencyAsync(currency1);
        await _fixture.Client.CreateCurrencyAsync(currency2);
        await _fixture.Client.CreateCurrencyAsync(currency3);

        // Act
        var response = await _fixture.Client.GetAllCurrenciesFilteredAsync(
            new GetAllCurrenciesFilteredRequest { Name = "Test" });

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Currencies.Count);
        Assert.Contains(response.Currencies, c => c.Currency.Name == "Test Currency One");
        Assert.Contains(response.Currencies, c => c.Currency.Name == "Test Currency Two");
    }

    [Fact]
    public async Task GetAllFilteredAsync_WithCaseInsensitiveNameMatch_ShouldReturnCurrencies()
    {
        // Arrange
        var currency = new CurrencyBuilder()
            .WithName("Test Currency")
            .WithDenominations(new List<Denomination>
            {
                new DenominationBuilder().WithValue(1.00M).Build()
            })
            .Build();

        await _fixture.Client.CreateCurrencyAsync(currency);

        // Act
        var response = await _fixture.Client.GetAllCurrenciesFilteredAsync(
            new GetAllCurrenciesFilteredRequest { Name = "test" });

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.Currencies);
        Assert.Contains(response.Currencies, c => c.Currency.Name == "Test Currency");
    }

    [Fact]
    public async Task GetAllFilteredAsync_WithDeletedCurrencies_ShouldNotReturnDeletedCurrencies()
    {
        // Arrange
        var currency1 = new CurrencyBuilder()
            .WithName("Test Currency One")
            .WithDenominations(new List<Denomination>
            {
                new DenominationBuilder().WithValue(1.00M).Build()
            })
            .Build();

        var currency2 = new CurrencyBuilder()
            .WithName("Test Currency Two")
            .WithDenominations(new List<Denomination>
            {
                new DenominationBuilder().WithValue(1.00M).Build()
            })
            .Build();

        var createResponse1 = await _fixture.Client.CreateCurrencyAsync(currency1);
        await _fixture.Client.CreateCurrencyAsync(currency2);

        var deleteRequest = new CurrencyRequestBuilder()
            .WithKey(createResponse1.Revision.Key)
            .Build();
        await _fixture.Client.DeleteCurrencyAsync(deleteRequest);

        // Act
        var response = await _fixture.Client.GetAllCurrenciesFilteredAsync(
            new GetAllCurrenciesFilteredRequest { Name = "Test" });

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.Currencies);
        Assert.Contains(response.Currencies, c => c.Currency.Name == "Test Currency Two");
    }

    [Fact]
    public async Task GetAllFilteredAsync_WithMultipleDenominations_ShouldReturnAllDenominations()
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

        await _fixture.Client.CreateCurrencyAsync(currency);

        // Act
        var response = await _fixture.Client.GetAllCurrenciesFilteredAsync(
            new GetAllCurrenciesFilteredRequest { Name = "Test" });

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.Currencies);
        var returnedCurrency = response.Currencies[0];
        Assert.Equal(2, returnedCurrency.Currency.Denominations.Count);
        Assert.Contains(returnedCurrency.Currency.Denominations,
            d => d.Denomination.Value == 1.00f && d.Denomination.Description == "One");
        Assert.Contains(returnedCurrency.Currency.Denominations,
            d => d.Denomination.Value == 2.00f && d.Denomination.Description == "Two");
    }

    [Fact]
    public async Task GetAllFilteredAsync_WithTooLongName_ShouldThrowInvalidArgument()
    {
        // Arrange
        var longName = new string('a', 101); // Exceeds 100 character limit

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.GetAllCurrenciesFilteredAsync(
                new GetAllCurrenciesFilteredRequest { Name = longName }));
        Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task GetAllFilteredAsync_WithInvalidName_ShouldThrowInvalidArgument(string name)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.GetAllCurrenciesFilteredAsync(
                new GetAllCurrenciesFilteredRequest { Name = name }));
        Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
    }
}