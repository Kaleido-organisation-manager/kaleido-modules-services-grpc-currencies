using Moq;
using Moq.AutoMock;
using Grpc.Core;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Services;
using Kaleido.Modules.Services.Grpc.Currencies.Create;
using Kaleido.Modules.Services.Grpc.Currencies.Delete;
using Kaleido.Modules.Services.Grpc.Currencies.Get;
using Kaleido.Modules.Services.Grpc.Currencies.GetAll;
using Kaleido.Modules.Services.Grpc.Currencies.GetAllFiltered;
using Kaleido.Modules.Services.Grpc.Currencies.GetAllRevisions;
using Kaleido.Modules.Services.Grpc.Currencies.GetRevision;
using Kaleido.Modules.Services.Grpc.Currencies.Update;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Common.Builders;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.Common.Services;

public class CurrencyServiceTests
{
    private readonly AutoMocker _mocker;
    private readonly CurrencyService _sut;

    public CurrencyServiceTests()
    {
        _mocker = new AutoMocker();
        _sut = _mocker.CreateInstance<CurrencyService>();
    }

    [Fact]
    public async Task CreateCurrency_CallsHandleAsyncOnCreateHandler()
    {
        // Arrange
        var request = new CurrencyBuilder().Build();
        var context = new Mock<ServerCallContext>().Object;

        // Act
        await _sut.CreateCurrency(request, context);

        // Assert
        _mocker.GetMock<ICreateHandler>()
            .Verify(x => x.HandleAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCurrency_CallsHandleAsyncOnDeleteHandler()
    {
        // Arrange
        var request = new CurrencyRequestBuilder().Build();
        var context = new Mock<ServerCallContext>().Object;

        // Act
        await _sut.DeleteCurrency(request, context);

        // Assert
        _mocker.GetMock<IDeleteHandler>()
            .Verify(x => x.HandleAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCurrency_CallsHandleAsyncOnGetHandler()
    {
        // Arrange
        var request = new CurrencyRequestBuilder().Build();
        var context = new Mock<ServerCallContext>().Object;

        // Act
        await _sut.GetCurrency(request, context);

        // Assert
        _mocker.GetMock<IGetHandler>()
            .Verify(x => x.HandleAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllCurrencies_CallsHandleAsyncOnGetAllHandler()
    {
        // Arrange
        var request = new EmptyRequest();
        var context = new Mock<ServerCallContext>().Object;

        // Act
        await _sut.GetAllCurrencies(request, context);

        // Assert
        _mocker.GetMock<IGetAllHandler>()
            .Verify(x => x.HandleAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllCurrenciesByName_CallsHandleAsyncOnGetAllByNameHandler()
    {
        // Arrange
        var request = new GetAllCurrenciesFilteredRequestBuilder().Build();
        var context = new Mock<ServerCallContext>().Object;

        // Act
        await _sut.GetAllCurrenciesFiltered(request, context);

        // Assert
        _mocker.GetMock<IGetAllFilteredHandler>()
            .Verify(x => x.HandleAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllCurrencyRevisions_CallsHandleAsyncOnGetAllRevisionsHandler()
    {
        // Arrange
        var request = new CurrencyRequestBuilder().Build();
        var context = new Mock<ServerCallContext>().Object;

        // Act
        await _sut.GetAllCurrencyRevisions(request, context);

        // Assert
        _mocker.GetMock<IGetAllRevisionsHandler>()
            .Verify(x => x.HandleAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCurrencyRevision_CallsHandleAsyncOnGetRevisionHandler()
    {
        // Arrange
        var request = new GetCurrencyRevisionRequestBuilder().Build();
        var context = new Mock<ServerCallContext>().Object;

        // Act
        await _sut.GetCurrencyRevision(request, context);

        // Assert
        _mocker.GetMock<IGetRevisionHandler>()
            .Verify(x => x.HandleAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCurrency_CallsHandleAsyncOnUpdateHandler()
    {
        // Arrange
        var request = new CurrencyActionRequestBuilder().Build();
        var context = new Mock<ServerCallContext>().Object;

        // Act
        await _sut.UpdateCurrency(request, context);

        // Assert
        _mocker.GetMock<IUpdateHandler>()
            .Verify(x => x.HandleAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }
}
