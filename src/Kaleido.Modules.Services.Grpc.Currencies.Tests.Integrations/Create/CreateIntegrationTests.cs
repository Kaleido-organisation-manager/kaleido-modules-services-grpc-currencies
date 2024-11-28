using Grpc.Core;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Integrations.Builders;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Integrations.Fixtures;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Integrations.Create;

[Collection("Infrastructure collection")]
public class CreateIntegrationTests
{
    private readonly InfrastructureFixture _fixture;

    public CreateIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearDatabase().Wait();
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateCurrency()
    {
        // Arrange
        var currency = new CurrencyBuilder().Build();

        // Act
        var response = await _fixture.Client.CreateCurrencyAsync(currency);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Currency);
        Assert.Equal(currency.Code, response.Currency.Code);
        Assert.Equal(currency.Name, response.Currency.Name);
        Assert.Equal(currency.Symbol, response.Currency.Symbol);
        Assert.NotNull(response.Revision);
        Assert.Equal("Created", response.Revision.Action);
        Assert.Equal(1, response.Revision.Revision);
        Assert.Single(response.Currency.Denominations);
        Assert.Equal(currency.Denominations[0].Value, response.Currency.Denominations[0].Denomination.Value);
    }

    [Fact]
    public async Task CreateAsync_WithMultipleDenominations_ShouldCreateCurrency()
    {
        // Arrange
        var denominations = new List<Denomination>()
        {
            new DenominationBuilder().WithValue(1).Build(),
            new DenominationBuilder().WithValue(2).Build(),
        };

        var currency = new CurrencyBuilder().WithDenominations(denominations).Build();

        // Act
        var response = await _fixture.Client.CreateCurrencyAsync(currency);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Currency.Denominations.Count);
        Assert.Contains(response.Currency.Denominations, p => p.Denomination.Value == denominations[0].Value);
        Assert.Contains(response.Currency.Denominations, p => p.Denomination.Value == denominations[1].Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task CreateAsync_InvalidName_ShouldThrow(string name)
    {
        // Arrange
        var currency = new CurrencyBuilder().WithName(name).Build();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.CreateCurrencyAsync(currency));
        Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
    }

    [Theory]
    [InlineData(-1)]
    public async Task CreateAsync_InvalidPriceValue_ShouldThrow(decimal value)
    {
        // Arrange
        var currency = new CurrencyBuilder().WithDenominations(new List<Denomination> {
            new DenominationBuilder().WithValue(value).Build(),
        }).Build();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.CreateCurrencyAsync(currency));
        Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_DescriptionTooLong_ShouldThrow()
    {
        // Arrange
        var currency = new CurrencyBuilder().WithName(new string('a', 1001)).Build();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.CreateCurrencyAsync(currency));
        Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_DuplicateDenominations_ShouldThrow()
    {
        // Arrange
        var currency = new CurrencyBuilder().WithDenominations(new List<Denomination> {
            new DenominationBuilder().WithValue(1).Build(),
            new DenominationBuilder().WithValue(1).Build(),
        }).Build();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.CreateCurrencyAsync(currency));
        Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_WithNonNaturalDenominationValue_ShouldReturnCorrectValue()
    {
        // Arrange
        var createCurrency = new CurrencyBuilder()
            .WithName("Test Currency")
            .WithDenominations(new List<Denomination>
            {
                new DenominationBuilder().WithValue(1.50M).Build(),
            })
            .Build();

        var createdCurrency = await _fixture.Client.CreateCurrencyAsync(createCurrency);

        Assert.NotNull(createdCurrency);
        Assert.Single(createdCurrency.Currency.Denominations);
        Assert.Equal(1.50f, createdCurrency.Currency.Denominations[0].Denomination.Value);
    }
}
