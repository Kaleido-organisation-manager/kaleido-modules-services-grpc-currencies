using Grpc.Core;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Integrations.Builders;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Integrations.Fixtures;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Integrations.GetAll;

[Collection("Infrastructure collection")]
public class GetAllIntegrationTests
{
    private readonly InfrastructureFixture _fixture;

    public GetAllIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearDatabase().Wait();
    }

    [Fact]
    public async Task GetAllAsync_EmptyDatabase_ShouldReturnEmptyList()
    {
        // Act
        var response = await _fixture.Client.GetAllCurrenciesAsync(new EmptyRequest());

        // Assert
        Assert.NotNull(response);
        Assert.Empty(response.Currencies);
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleCurrencies_ShouldReturnAllCurrencies()
    {
        // Arrange
        var currency1 = new CurrencyBuilder()
            .WithName("Test Currency 1")
            .WithCode("TC1")
            .WithDenominations(new List<Denomination>
            {
                new DenominationBuilder().WithValue(1.00M).Build()
            })
            .Build();

        var currency2 = new CurrencyBuilder()
            .WithName("Test Currency 2")
            .WithCode("TC2")
            .WithDenominations(new List<Denomination>
            {
                new DenominationBuilder().WithValue(2.00M).Build()
            })
            .Build();

        await _fixture.Client.CreateCurrencyAsync(currency1);
        await _fixture.Client.CreateCurrencyAsync(currency2);

        // Act
        var response = await _fixture.Client.GetAllCurrenciesAsync(new EmptyRequest());

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Currencies.Count);
        Assert.Contains(response.Currencies, c => c.Currency.Name == "Test Currency 1" && c.Currency.Code == "TC1");
        Assert.Contains(response.Currencies, c => c.Currency.Name == "Test Currency 2" && c.Currency.Code == "TC2");
    }

    [Fact]
    public async Task GetAllAsync_WithDeletedCurrencies_ShouldNotReturnDeletedCurrencies()
    {
        // Arrange
        var currency1 = new CurrencyBuilder()
            .WithName("Test Currency 1")
            .WithDenominations(new List<Denomination>
            {
                new DenominationBuilder().WithValue(1.00M).Build()
            })
            .Build();

        var currency2 = new CurrencyBuilder()
            .WithName("Test Currency 2")
            .WithDenominations(new List<Denomination>
            {
                new DenominationBuilder().WithValue(2.00M).Build()
            })
            .Build();

        var createResponse1 = await _fixture.Client.CreateCurrencyAsync(currency1);
        await _fixture.Client.CreateCurrencyAsync(currency2);

        var deleteRequest = new CurrencyRequestBuilder()
            .WithKey(createResponse1.Revision.Key)
            .Build();
        await _fixture.Client.DeleteCurrencyAsync(deleteRequest);

        // Act
        var response = await _fixture.Client.GetAllCurrenciesAsync(new EmptyRequest());

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.Currencies);
        Assert.Contains(response.Currencies, c => c.Currency.Name == "Test Currency 2");
        Assert.DoesNotContain(response.Currencies, c => c.Currency.Name == "Test Currency 1");
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleDenominationsPerCurrency_ShouldReturnAllDenominations()
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
        var response = await _fixture.Client.GetAllCurrenciesAsync(new EmptyRequest());

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
    public async Task GetAllAsync_WithDeletedDenominations_ShouldNotReturnDeletedDenominations()
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

        // Update currency with new denomination
        var updatedCurrency = new CurrencyBuilder()
            .WithName("Test Currency")
            .WithDenominations(new List<Denomination>
            {
                new DenominationBuilder().WithValue(2.00M).Build()
            })
            .Build();

        await _fixture.Client.UpdateCurrencyAsync(new CurrencyActionRequest
        {
            Key = createResponse.Revision.Key,
            Currency = updatedCurrency
        });

        // Act
        var response = await _fixture.Client.GetAllCurrenciesAsync(new EmptyRequest());

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.Currencies);
        var returnedCurrency = response.Currencies[0];
        Assert.Single(returnedCurrency.Currency.Denominations);
        Assert.Equal(2.00f, returnedCurrency.Currency.Denominations[0].Denomination.Value);
    }
}