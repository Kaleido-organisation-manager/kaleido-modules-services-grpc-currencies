using Moq;
using Moq.AutoMock;
using Kaleido.Modules.Services.Grpc.Currencies.GetAllFiltered;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using System.Linq.Expressions;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Common.Builders;

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

            var currencies = new List<EntityLifeCycleResult<CurrencyEntity, BaseRevisionEntity>>
            {
                new EntityLifeCycleResult<CurrencyEntity, BaseRevisionEntity>
                {
                    Entity = new CurrencyEntityBuilder().Build(),
                    Revision = new CurrencyRevisionBuilder().WithKey(Guid.NewGuid()).Build()
                },
                new EntityLifeCycleResult<CurrencyEntity, BaseRevisionEntity>
                {
                    Entity = new CurrencyEntityBuilder().WithName("Dollar").WithCode("USD").WithSymbol("$").Build(),
                    Revision = new CurrencyRevisionBuilder().WithKey(Guid.NewGuid()).Build()
                }
            };


            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity>>()
                .Setup(r => r.FindAllAsync(
                    It.IsAny<Expression<Func<CurrencyEntity, bool>>>(),
                    It.IsAny<Expression<Func<BaseRevisionEntity, bool>>>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(currencies);
        }

        [Fact]
        public async Task GetAllByNameAsync_ShouldCallHandler()
        {
            // Arrange
            var name = "Euro";

            // Act
            await _sut.GetAllByNameAsync(name);

            // Assert
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity>>()
                .Verify(r => r.FindAllAsync(
                    It.IsAny<Expression<Func<CurrencyEntity, bool>>>(),
                    It.IsAny<Expression<Func<BaseRevisionEntity, bool>>>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAllByNameAsync_ShouldReturnMappedCurrencies()
        {
            // Arrange
            var name = "Euro";

            // Act
            var result = await _sut.GetAllByNameAsync(name);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task GetAllByNameAsync_ShouldPassCancellationTokenToRepository()
        {
            // Arrange
            var name = "Euro";
            var cancellationToken = new CancellationToken();

            // Act
            await _sut.GetAllByNameAsync(name, cancellationToken);

            // Assert
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity>>()
                .Verify(r => r.FindAllAsync(
                    It.IsAny<Expression<Func<CurrencyEntity, bool>>>(),
                    It.IsAny<Expression<Func<BaseRevisionEntity, bool>>>(),
                    It.IsAny<Guid?>(),
                    cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetAllByNameAsync_WhenRepositoryReturnsEmptyList_ShouldReturnEmptyList()
        {
            // Arrange
            var name = "Euro";
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity>>()
                .Setup(r => r.FindAllAsync(
                    It.IsAny<Expression<Func<CurrencyEntity, bool>>>(),
                    It.IsAny<Expression<Func<BaseRevisionEntity, bool>>>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EntityLifeCycleResult<CurrencyEntity, BaseRevisionEntity>>());

            // Act
            var result = await _sut.GetAllByNameAsync(name);

            // Assert
            Assert.Empty(result);
        }
    }
}

