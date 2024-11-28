using Grpc.Core;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Integrations.Builders;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Integrations.Fixtures;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Integrations.Delete;

[Collection("Infrastructure collection")]
public class DeleteIntegrationTests
{
    private readonly InfrastructureFixture _fixture;

    public DeleteIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearDatabase().Wait();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteCurrency()
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
        var deleteResponse = await _fixture.Client.DeleteCurrencyAsync(request);

        // Assert
        Assert.NotNull(deleteResponse);
        Assert.Equal(createResponse.Revision.Key, deleteResponse.Revision.Key);
        Assert.Equal("Deleted", deleteResponse.Revision.Action);
        Assert.Equal(2, deleteResponse.Revision.Revision);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentKey_ShouldThrowNotFound()
    {
        // Arrange
        var request = new CurrencyRequestBuilder()
            .WithKey(Guid.NewGuid().ToString())
            .Build();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.DeleteCurrencyAsync(request));
        Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid-guid")]
    public async Task DeleteAsync_WithInvalidKey_ShouldThrowInvalidArgument(string key)
    {
        // Arrange
        var request = new CurrencyRequestBuilder()
            .WithKey(key)
            .Build();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.DeleteCurrencyAsync(request));
        Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteAssociatedDenominations()
    {
        // Arrange
        var currency = new CurrencyBuilder()
            .WithName("Test Currency")
            .WithDenominations(new List<Denomination>
            {
                new DenominationBuilder().WithValue(1.00M).Build(),
                new DenominationBuilder().WithValue(2.00M).Build()
            })
            .Build();

        var createResponse = await _fixture.Client.CreateCurrencyAsync(currency);
        var request = new CurrencyRequestBuilder()
            .WithKey(createResponse.Revision.Key)
            .Build();

        // Act
        var deleteResponse = await _fixture.Client.DeleteCurrencyAsync(request);

        // Assert
        Assert.NotNull(deleteResponse);
        Assert.Equal(createResponse.Revision.Key, deleteResponse.Revision.Key);
        Assert.Equal("Deleted", deleteResponse.Revision.Action);
        Assert.Equal(2, deleteResponse.Revision.Revision);

        // Verify all denominations are marked as deleted
        foreach (var denomination in deleteResponse.Currency.Denominations)
        {
            Assert.Equal("Deleted", denomination.Revision.Action);
            Assert.Equal(2, denomination.Revision.Revision);
        }
    }

    [Fact]
    public async Task DeleteAsync_AlreadyDeletedCurrency_ShouldThrowNotFound()
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
            async () => await _fixture.Client.DeleteCurrencyAsync(request));
        Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
    }
}
