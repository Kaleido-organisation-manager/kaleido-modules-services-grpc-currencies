using Grpc.Core;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Integrations.Builders;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Integrations.Fixtures;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Integrations.Update;

[Collection("Infrastructure collection")]
public class UpdateIntegrationTests
{
    private readonly InfrastructureFixture _fixture;

    public UpdateIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearDatabase().Wait();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCurrency()
    {
        // Arrange
        var createCurrency = new CurrencyBuilder()
            .WithName("Initial Currency")
            .WithCode("INI")
            .WithSymbol("I")
            .WithDenominations(new List<Denomination>
            {
                new DenominationBuilder().WithValue(1.00M).WithDescription("One").Build()
            })
            .Build();

        var createdCurrency = await _fixture.Client.CreateCurrencyAsync(createCurrency);

        var updateRequest = new CurrencyActionRequest
        {
            Key = createdCurrency.Revision.Key,
            Currency = new CurrencyBuilder()
                .WithName("Updated Currency")
                .WithCode("UPD")
                .WithSymbol("U")
                .WithDenominations(new List<Denomination>
                {
                    new DenominationBuilder().WithValue(2.00M).WithDescription("Two").Build()
                })
                .Build()
        };

        // Act
        var response = await _fixture.Client.UpdateCurrencyAsync(updateRequest);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Updated Currency", response.Currency.Name);
        Assert.Equal("UPD", response.Currency.Code);
        Assert.Equal("U", response.Currency.Symbol);
        Assert.Single(response.Currency.Denominations.Where(x => x.Revision.Action != "Deleted"));
        Assert.Single(response.Currency.Denominations.Where(x => x.Revision.Action == "Created"));
        Assert.Equal(2.00f, response.Currency.Denominations
            .First(x => x.Revision.Action == "Created").Denomination.Value);
    }

    [Fact]
    public async Task UpdateAsync_DenominationIsRemoved_ShouldMarkAsDeleted()
    {
        // Arrange
        var createCurrency = new CurrencyBuilder()
            .WithName("Test Currency")
            .WithDenominations(new List<Denomination>
            {
                new DenominationBuilder().WithValue(1.00M).WithDescription("One").Build(),
                new DenominationBuilder().WithValue(2.00M).WithDescription("Two").Build()
            })
            .Build();

        var createdCurrency = await _fixture.Client.CreateCurrencyAsync(createCurrency);

        var updateRequest = new CurrencyActionRequest
        {
            Key = createdCurrency.Revision.Key,
            Currency = new CurrencyBuilder()
                .WithName("Test Currency")
                .WithDenominations(new List<Denomination>
                {
                    new DenominationBuilder().WithValue(1.00M).WithDescription("One").Build()
                })
                .Build()
        };

        // Act
        var response = await _fixture.Client.UpdateCurrencyAsync(updateRequest);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Currency.Denominations.Count);
        Assert.Single(response.Currency.Denominations.Where(x => x.Revision.Action == "Unmodified"));
        Assert.Equal(1.00f, response.Currency.Denominations
            .First(x => x.Revision.Action == "Unmodified").Denomination.Value);
        Assert.Equal("Deleted", response.Currency.Denominations
            .First(x => x.Denomination.Value == 2.00f).Revision.Action);
    }

    [Fact]
    public async Task UpdateAsync_WhenDenominationIsRestored_ShouldBeMarkedAsRestored()
    {
        // Arrange
        var createCurrency = new CurrencyBuilder()
            .WithName("Test Currency")
            .WithDenominations(new List<Denomination>
            {
                new DenominationBuilder().WithValue(1.00M).WithDescription("One").Build(),
                new DenominationBuilder().WithValue(2.00M).WithDescription("Two").Build()
            })
            .Build();

        var createdCurrency = await _fixture.Client.CreateCurrencyAsync(createCurrency);

        // First update - remove a denomination
        var updateRequest1 = new CurrencyActionRequest
        {
            Key = createdCurrency.Revision.Key,
            Currency = new CurrencyBuilder()
                .WithName("Test Currency")
                .WithDenominations(new List<Denomination>
                {
                    new DenominationBuilder().WithValue(1.00M).WithDescription("One").Build()
                })
                .Build()
        };

        await _fixture.Client.UpdateCurrencyAsync(updateRequest1);

        // Second update - restore the denomination
        var updateRequest2 = new CurrencyActionRequest
        {
            Key = createdCurrency.Revision.Key,
            Currency = new CurrencyBuilder()
                .WithName("Test Currency")
                .WithDenominations(new List<Denomination>
                {
                    new DenominationBuilder().WithValue(1.00M).WithDescription("One").Build(),
                    new DenominationBuilder().WithValue(2.00M).WithDescription("Two").Build()
                })
                .Build()
        };

        // Act
        var response = await _fixture.Client.UpdateCurrencyAsync(updateRequest2);

        // Assert
        Assert.NotNull(response);
        Assert.Contains(response.Currency.Denominations,
            d => d.Denomination.Value == 2.00f && d.Revision.Action == "Restored");
        Assert.Equal(2, response.Currency.Denominations.Count);
    }

    [Fact]
    public async Task UpdateAsync_WithDenominationDescriptionUpdate_ShouldUpdateDenomination()
    {
        // Arrange
        var createCurrency = new CurrencyBuilder()
            .WithName("Test Currency")
            .WithDenominations(new List<Denomination>
            {
                new DenominationBuilder()
                    .WithValue(1.00M)
                    .WithDescription("One")
                    .Build()
            })
            .Build();

        var createdCurrency = await _fixture.Client.CreateCurrencyAsync(createCurrency);

        var updateRequest = new CurrencyActionRequest
        {
            Key = createdCurrency.Revision.Key,
            Currency = new CurrencyBuilder()
                .WithName("Test Currency")
                .WithDenominations(new List<Denomination>
                {
                    new DenominationBuilder()
                        .WithValue(1.00M)
                        .WithDescription("Updated One")
                        .Build()
                })
                .Build()
        };

        // Act
        var response = await _fixture.Client.UpdateCurrencyAsync(updateRequest);

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.Currency.Denominations);
        Assert.Equal(1.00f, response.Currency.Denominations[0].Denomination.Value);
        Assert.Equal("Updated One", response.Currency.Denominations[0].Denomination.Description);
        Assert.Equal("Updated", response.Currency.Denominations[0].Revision.Action);
    }

    [Fact]
    public async Task UpdateAsync_WithDenominationValueUpdate_ShouldCreateNewDenomination()
    {
        // Arrange
        var createCurrency = new CurrencyBuilder()
            .WithName("Test Currency")
            .WithDenominations(new List<Denomination>
            {
                new DenominationBuilder()
                    .WithValue(1.00M)
                    .WithDescription("One")
                    .Build()
            })
            .Build();

        var createdCurrency = await _fixture.Client.CreateCurrencyAsync(createCurrency);

        var updateRequest = new CurrencyActionRequest
        {
            Key = createdCurrency.Revision.Key,
            Currency = new CurrencyBuilder()
                .WithName("Test Currency")
                .WithDenominations(new List<Denomination>
                {
                    new DenominationBuilder()
                        .WithValue(2.00M)
                        .WithDescription("One")
                        .Build()
                })
                .Build()
        };

        // Act
        var response = await _fixture.Client.UpdateCurrencyAsync(updateRequest);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Currency.Denominations.Count);
        Assert.Single(response.Currency.Denominations.Where(x => x.Revision.Action == "Created"));
        Assert.Single(response.Currency.Denominations.Where(x => x.Revision.Action == "Deleted"));
        Assert.Equal(2.00f, response.Currency.Denominations.Where(x => x.Revision.Action == "Created").First().Denomination.Value);
        Assert.Equal("One", response.Currency.Denominations.Where(x => x.Revision.Action == "Created").First().Denomination.Description);
        Assert.Equal("Created", response.Currency.Denominations.Where(x => x.Revision.Action == "Created").First().Revision.Action);

    }

    [Fact]
    public async Task UpdateAsync_WithUnchangedDenomination_ShouldMarkAsUnmodified()
    {
        // Arrange
        var createCurrency = new CurrencyBuilder()
            .WithName("Test Currency")
            .WithDenominations(new List<Denomination>
            {
                new DenominationBuilder()
                    .WithValue(1.00M)
                    .WithDescription("One")
                    .Build()
            })
            .Build();

        var createdCurrency = await _fixture.Client.CreateCurrencyAsync(createCurrency);

        var updateRequest = new CurrencyActionRequest
        {
            Key = createdCurrency.Revision.Key,
            Currency = new CurrencyBuilder()
                .WithName("Updated Currency") // Change currency name but keep same denomination
                .WithDenominations(new List<Denomination>
                {
                    new DenominationBuilder()
                        .WithValue(1.00M)
                        .WithDescription("One")
                        .Build()
                })
                .Build()
        };

        // Act
        var response = await _fixture.Client.UpdateCurrencyAsync(updateRequest);

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.Currency.Denominations);
        Assert.Equal(1.00f, response.Currency.Denominations[0].Denomination.Value);
        Assert.Equal("One", response.Currency.Denominations[0].Denomination.Description);
        Assert.Equal("Unmodified", response.Currency.Denominations[0].Revision.Action);
    }

    [Fact]
    public async Task UpdateAsync_CurrencyDoesNotExist_ShouldThrow()
    {
        // Arrange
        var updateRequest = new CurrencyActionRequest
        {
            Key = Guid.NewGuid().ToString(),
            Currency = new CurrencyBuilder().Build()
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.UpdateCurrencyAsync(updateRequest));
        Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
    }

    [Theory]
    [InlineData("invalid-guid")]
    [InlineData("")]
    public async Task UpdateAsync_InvalidKey_ShouldThrow(string key)
    {
        // Arrange
        var updateRequest = new CurrencyActionRequest
        {
            Key = key,
            Currency = new CurrencyBuilder().Build()
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.UpdateCurrencyAsync(updateRequest));
        Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
    }

    [Fact]
    public async Task UpdateAsync_OnUpdate_ShouldUseSharedTimestamp()
    {
        // Arrange
        var createCurrency = new CurrencyBuilder()
            .WithName("Test Currency")
            .WithDenominations(new List<Denomination>
            {
                new DenominationBuilder().WithValue(1.00M).Build(),
                new DenominationBuilder().WithValue(2.00M).Build()
            })
            .Build();

        var createdCurrency = await _fixture.Client.CreateCurrencyAsync(createCurrency);

        var updateRequest = new CurrencyActionRequest
        {
            Key = createdCurrency.Revision.Key,
            Currency = new CurrencyBuilder()
                .WithName("Updated Currency")
                .WithDenominations(new List<Denomination>
                {
                    new DenominationBuilder().WithValue(5.00M).Build()
                })
                .Build()
        };

        // Act
        var response = await _fixture.Client.UpdateCurrencyAsync(updateRequest);

        // Assert
        var updateTimestamps = response.Currency.Denominations
            .Select(d => d.Revision.CreatedAt)
            .Concat(new[] { response.Revision.CreatedAt })
            .ToList();

        Assert.Single(updateTimestamps.Distinct());
    }

}
