using Moq;
using Moq.AutoMock;
using Kaleido.Modules.Services.Grpc.Currencies.GetAllFiltered;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using System.Linq.Expressions;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.Builders;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.GetAllFiltered
{
    public class GetAllFilteredManagerTests
    {
        private readonly AutoMocker _mocker;
        private readonly GetAllFilteredManager _sut;

        public GetAllFilteredManagerTests()
        {
            _mocker = new AutoMocker();
            _sut = _mocker.CreateInstance<GetAllFilteredManager>();

            var currencies = new List<EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity>>
            {
                new EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity>
                {
                    Entity = new CurrencyEntityBuilder().Build(),
                    Revision = new CurrencyRevisionBuilder().WithKey(Guid.NewGuid()).Build()
                },
                new EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity>
                {
                    Entity = new CurrencyEntityBuilder().WithName("Dollar").WithCode("USD").WithSymbol("$").Build(),
                    Revision = new CurrencyRevisionBuilder().WithKey(Guid.NewGuid()).Build()
                }
            };

            var denominations = new List<EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>>
            {
                new EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>
                {
                    Entity = new DenominationEntityBuilder().Build(),
                    Revision = new DenominationRevisionBuilder().Build()
                }
            };


            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity>>()
                .Setup(r => r.FindAllAsync(
                    It.IsAny<Expression<Func<CurrencyEntity, bool>>>(),
                    It.IsAny<Expression<Func<CurrencyRevisionEntity, bool>>>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(currencies);

            _mocker.GetMock<IEntityLifecycleHandler<DenominationEntity, DenominationRevisionEntity>>()
                .Setup(r => r.FindAllAsync(
                    It.IsAny<Expression<Func<DenominationEntity, bool>>>(),
                    It.IsAny<Expression<Func<DenominationRevisionEntity, bool>>>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(denominations);
        }

        [Fact]
        public async Task GetAllFilteredAsync_ShouldCallHandler()
        {
            // Arrange
            var name = "Euro";

            // Act
            await _sut.GetAllFilteredAsync(name);

            // Assert
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity>>()
                .Verify(r => r.FindAllAsync(
                    It.IsAny<Expression<Func<CurrencyEntity, bool>>>(),
                    It.IsAny<Expression<Func<CurrencyRevisionEntity, bool>>>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAllFilteredAsync_ShouldReturnMappedCurrencies()
        {
            // Arrange
            var name = "Euro";

            // Act
            var result = await _sut.GetAllFilteredAsync(name);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task GetAllFilteredAsync_ShouldPassCancellationTokenToRepository()
        {
            // Arrange
            var name = "Euro";
            var cancellationToken = new CancellationToken();

            // Act
            await _sut.GetAllFilteredAsync(name, cancellationToken);

            // Assert
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity>>()
                .Verify(r => r.FindAllAsync(
                    It.IsAny<Expression<Func<CurrencyEntity, bool>>>(),
                    It.IsAny<Expression<Func<CurrencyRevisionEntity, bool>>>(),
                    It.IsAny<Guid?>(),
                    cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetAllFilteredAsync_WhenRepositoryReturnsEmptyList_ShouldReturnEmptyList()
        {
            // Arrange
            var name = "Euro";
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity>>()
                .Setup(r => r.FindAllAsync(
                    It.IsAny<Expression<Func<CurrencyEntity, bool>>>(),
                    It.IsAny<Expression<Func<CurrencyRevisionEntity, bool>>>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity>>());

            // Act
            var result = await _sut.GetAllFilteredAsync(name);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllFilteredAsync_ShouldReturnDenominations()
        {
            // Arrange
            var name = "Euro";

            // Act
            var result = await _sut.GetAllFilteredAsync(name);

            // Assert
            Assert.NotNull(result.First().Denominations);
            Assert.NotEmpty(result.First().Denominations!);
        }

        [Fact]
        public async Task GetAllFilteredAsync_ShouldCallFindAllAsyncOnDenominationHandler()
        {
            // Arrange
            var name = "Euro";

            // Act
            await _sut.GetAllFilteredAsync(name);

            // Assert
            _mocker.GetMock<IEntityLifecycleHandler<DenominationEntity, DenominationRevisionEntity>>()
                .Verify(r => r.FindAllAsync(
                    It.IsAny<Expression<Func<DenominationEntity, bool>>>(),
                    It.IsAny<Expression<Func<DenominationRevisionEntity, bool>>>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()), Times.Exactly(2));
        }
    }
}

