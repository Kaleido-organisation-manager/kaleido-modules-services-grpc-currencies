using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Integrations.Builders;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Integrations.Fixtures;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Integrations.GetRevision;

[Collection("Infrastructure collection")]
public class GetRevisionIntegrationTests
{
    private readonly InfrastructureFixture _fixture;

    public GetRevisionIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearDatabase().Wait();
    }

    [Fact]
    public async Task GetRevision_WhenCurrencyExists_ReturnsCurrencyRevision()
    {
        // Arrange
        var createCurrency = new CurrencyBuilder()
            .WithName("Initial Currency")
            .WithCode("INI")
            .WithSymbol("I")
            .WithDenominations(new List<Denomination>
            {
                new DenominationBuilder().WithValue(1.00M).WithDescription("One").Build(),
                new DenominationBuilder().WithValue(2.00M).WithDescription("Two").Build()
            })
            .Build();

        var createdCurrency = await _fixture.Client.CreateCurrencyAsync(createCurrency);

        var updatedCurrency = new CurrencyBuilder()
            .WithName("Updated Currency")
            .WithCode("UPD")
            .WithSymbol("U")
            .WithDenominations(new List<Denomination>
            {
                new DenominationBuilder().WithValue(5.00M).WithDescription("Five").Build()
            })
            .Build();

        await _fixture.Client.UpdateCurrencyAsync(new CurrencyActionRequest
        {
            Key = createdCurrency.Revision.Key,
            Currency = updatedCurrency
        });

        // Act
        var request = new GetCurrencyRevisionRequest
        {
            Key = createdCurrency.Revision.Key,
            CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
        };
        var response = await _fixture.Client.GetCurrencyRevisionAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Updated Currency", response.Currency.Name);
        Assert.Equal("UPD", response.Currency.Code);
        Assert.Equal("U", response.Currency.Symbol);
        Assert.Single(response.Currency.Denominations.Where(x => x.Revision.Action != "Deleted"));
        Assert.Equal(2, response.Currency.Denominations.Count(x => x.Revision.Action == "Deleted"));
        Assert.Equal(5.00f, response.Currency.Denominations
            .First(x => x.Revision.Action != "Deleted").Denomination.Value);
    }

    [Fact]
    public async Task GetRevision_WhenCurrencyDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var request = new GetCurrencyRevisionRequest
        {
            Key = Guid.NewGuid().ToString(),
            CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.GetCurrencyRevisionAsync(request));
        Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
    }

    [Theory]
    [InlineData("invalid-guid")]
    [InlineData("")]
    public async Task GetRevision_WithInvalidKey_ThrowsInvalidArgumentException(string key)
    {
        // Arrange
        var request = new GetCurrencyRevisionRequest
        {
            Key = key,
            CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.GetCurrencyRevisionAsync(request));
        Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
    }

    [Fact]
    public async Task GetRevision_WhenRevisionDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var currency = new CurrencyBuilder()
            .WithName("Test Currency")
            .WithDenominations(new List<Denomination>
            {
                new DenominationBuilder().WithValue(1.00M).Build()
            })
            .Build();

        var createdCurrency = await _fixture.Client.CreateCurrencyAsync(currency);

        var request = new GetCurrencyRevisionRequest
        {
            Key = createdCurrency.Revision.Key,
            CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-1)) // Past date with no revisions
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.GetCurrencyRevisionAsync(request));
        Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
    }

    [Fact]
    public async Task GetRevision_WithDeletedCurrency_ReturnsDeletedRevision()
    {
        // Arrange
        var currency = new CurrencyBuilder()
            .WithName("Test Currency")
            .WithDenominations(new List<Denomination>
            {
                new DenominationBuilder().WithValue(1.00M).Build()
            })
            .Build();

        var createdCurrency = await _fixture.Client.CreateCurrencyAsync(currency);
        await _fixture.Client.DeleteCurrencyAsync(new CurrencyRequestBuilder()
            .WithKey(createdCurrency.Revision.Key)
            .Build());

        // Act
        var request = new GetCurrencyRevisionRequest
        {
            Key = createdCurrency.Revision.Key,
            CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
        };
        var response = await _fixture.Client.GetCurrencyRevisionAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Deleted", response.Revision.Action);
    }

    [Fact]
    public async Task GetRevision_WithMultipleDenominationChanges_ReturnsCorrectDenominationState()
    {
        // Arrange
        var initialCurrency = new CurrencyBuilder()
            .WithName("Test Currency")
            .WithDenominations(new List<Denomination>
            {
                new DenominationBuilder().WithValue(1.00M).WithDescription("One").Build(),
                new DenominationBuilder().WithValue(2.00M).WithDescription("Two").Build()
            })
            .Build();

        var createdCurrency = await _fixture.Client.CreateCurrencyAsync(initialCurrency);
        var initialTimestamp = DateTime.UtcNow;

        // Wait a bit to ensure different timestamps
        await Task.Delay(100);

        var updateRequest = new CurrencyActionRequest
        {
            Key = createdCurrency.Revision.Key,
            Currency = new CurrencyBuilder()
                .WithName("Test Currency")
                .WithDenominations(new List<Denomination>
                {
                    new DenominationBuilder().WithValue(5.00M).WithDescription("Five").Build()
                })
                .Build()
        };

        await _fixture.Client.UpdateCurrencyAsync(updateRequest);

        // Act
        var request = new GetCurrencyRevisionRequest
        {
            Key = createdCurrency.Revision.Key,
            CreatedAt = Timestamp.FromDateTime(initialTimestamp)
        };
        var response = await _fixture.Client.GetCurrencyRevisionAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Currency.Denominations.Count);
        Assert.Contains(response.Currency.Denominations,
            d => d.Denomination.Value == 1.00f && d.Denomination.Description == "One");
        Assert.Contains(response.Currency.Denominations,
            d => d.Denomination.Value == 2.00f && d.Denomination.Description == "Two");
    }
}