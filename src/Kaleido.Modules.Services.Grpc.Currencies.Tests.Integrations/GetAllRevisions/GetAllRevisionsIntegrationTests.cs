using Grpc.Core;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Integrations.Builders;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Integrations.Fixtures;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Integrations.GetAllRevisions;

[Collection("Infrastructure collection")]
public class GetAllRevisionsIntegrationTests
{
    private readonly InfrastructureFixture _fixture;

    public GetAllRevisionsIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearDatabase().Wait();
    }

    [Fact]
    public async Task GetAllRevisions_WhenCurrencyAndDenominationsExist_ReturnsCurrencyListResponse()
    {
        // Arrange
        var createCurrency = new CurrencyBuilder()
            .WithName("Test Currency")
            .WithCode("TST")
            .WithSymbol("T")
            .WithDenominations(new List<Denomination>
            {
                new DenominationBuilder().WithValue(1.00M).WithDescription("One").Build(),
                new DenominationBuilder().WithValue(2.00M).WithDescription("Two").Build()
            })
            .Build();

        var createdCurrency = await _fixture.Client.CreateCurrencyAsync(createCurrency);

        // Create revision by updating the currency
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
        var request = new CurrencyRequestBuilder()
            .WithKey(createdCurrency.Revision.Key)
            .Build();
        var response = await _fixture.Client.GetAllCurrencyRevisionsAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.NotEmpty(response.Currencies);
        Assert.Equal(2, response.Currencies.Count); // Expecting 2 revisions

        var latestRevision = response.Currencies[0];
        var initialRevision = response.Currencies[1];

        // Check initial revision
        Assert.Equal("Test Currency", initialRevision.Currency.Name);
        Assert.Equal("TST", initialRevision.Currency.Code);
        Assert.Equal("T", initialRevision.Currency.Symbol);
        Assert.Equal(2, initialRevision.Currency.Denominations.Count);
        Assert.Equal("Created", initialRevision.Revision.Action);

        // Check latest revision
        Assert.Equal("Updated Currency", latestRevision.Currency.Name);
        Assert.Equal("UPD", latestRevision.Currency.Code);
        Assert.Equal("U", latestRevision.Currency.Symbol);
        Assert.Equal(3, latestRevision.Currency.Denominations.Count);
        Assert.Equal(2, latestRevision.Currency.Denominations.Count(x => x.Revision.Action == "Deleted"));
        Assert.Single(latestRevision.Currency.Denominations.Where(x => x.Revision.Action == "Created"));
        Assert.Equal("Updated", latestRevision.Revision.Action);
    }

    [Fact]
    public async Task GetAllRevisions_WhenNoRevisionsExist_ReturnsEmptyResponse()
    {
        // Arrange
        var request = new CurrencyRequestBuilder()
            .WithKey(Guid.NewGuid().ToString())
            .Build();

        // Act
        var response = await _fixture.Client.GetAllCurrencyRevisionsAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Empty(response.Currencies);
    }

    [Fact]
    public async Task GetAllRevisions_WhenCurrencyIsDeleted_IncludesDeletedRevision()
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
        var request = new CurrencyRequestBuilder()
            .WithKey(createdCurrency.Revision.Key)
            .Build();
        var response = await _fixture.Client.GetAllCurrencyRevisionsAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Currencies.Count);
        Assert.Equal("Deleted", response.Currencies[0].Revision.Action);
        Assert.Equal("Created", response.Currencies[1].Revision.Action);
    }

    [Fact]
    public async Task GetAllRevisions_ReflectsDenominationChanges()
    {
        // Arrange
        var initialCurrency = new CurrencyBuilder()
            .WithName("Test Currency 1")
            .WithDenominations(new List<Denomination>
            {
                new DenominationBuilder().WithValue(1.00M).WithDescription("One").Build(),
                new DenominationBuilder().WithValue(2.00M).WithDescription("Two").Build()
            })
            .Build();

        var createdCurrency = await _fixture.Client.CreateCurrencyAsync(initialCurrency);

        var firstUpdate = new CurrencyBuilder()
            .WithName("Test Currency 2")
            .WithDenominations(new List<Denomination>
            {
                new DenominationBuilder().WithValue(5.00M).WithDescription("Five").Build()
            })
            .Build();

        await _fixture.Client.UpdateCurrencyAsync(new CurrencyActionRequest
        {
            Key = createdCurrency.Revision.Key,
            Currency = firstUpdate
        });

        var secondUpdate = new CurrencyBuilder()
            .WithName("Test Currency 2")
            .WithDenominations(new List<Denomination>
            {
                new DenominationBuilder().WithValue(5.00M).WithDescription("Five").Build(),
                new DenominationBuilder().WithValue(10.00M).WithDescription("Ten").Build()
            })
            .Build();

        await _fixture.Client.UpdateCurrencyAsync(new CurrencyActionRequest
        {
            Key = createdCurrency.Revision.Key,
            Currency = secondUpdate
        });

        // Act
        var request = new CurrencyRequestBuilder()
            .WithKey(createdCurrency.Revision.Key)
            .Build();
        var response = await _fixture.Client.GetAllCurrencyRevisionsAsync(request);

        // Assert
        Assert.Equal(3, response.Currencies.Count);

        var latestRevision = response.Currencies[0];
        var middleRevision = response.Currencies[1];
        var initialRevision = response.Currencies[2];

        // Check initial revision
        Assert.Equal(2, initialRevision.Currency.Denominations.Count);
        Assert.Equal("Created", initialRevision.Revision.Action);
        Assert.All(initialRevision.Currency.Denominations, d => Assert.Equal("Created", d.Revision.Action));

        // Check middle revision
        Assert.Equal(3, middleRevision.Currency.Denominations.Count);
        Assert.Equal("Updated", middleRevision.Revision.Action);
        Assert.Single(middleRevision.Currency.Denominations.Where(x => x.Revision.Action == "Created"));
        Assert.Equal(2, middleRevision.Currency.Denominations.Count(x => x.Revision.Action == "Deleted"));
        Assert.Equal(5.00f, middleRevision.Currency.Denominations.Where(x => x.Revision.Action == "Created").First().Denomination.Value);

        // Check latest revision
        Assert.Equal(2, latestRevision.Currency.Denominations.Count);
        Assert.Equal("Unmodified", latestRevision.Revision.Action);
        Assert.Single(latestRevision.Currency.Denominations.Where(x => x.Revision.Action == "Created"));
        Assert.Single(latestRevision.Currency.Denominations.Where(x => x.Revision.Action == "Unmodified"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid-guid")]
    public async Task GetAllRevisions_WithInvalidKey_ThrowsInvalidArgument(string key)
    {
        // Arrange
        var request = new CurrencyRequestBuilder()
            .WithKey(key)
            .Build();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.GetAllCurrencyRevisionsAsync(request));
        Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
    }
}