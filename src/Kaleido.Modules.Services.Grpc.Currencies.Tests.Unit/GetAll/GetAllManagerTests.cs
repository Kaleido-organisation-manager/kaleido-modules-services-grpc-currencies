using Moq;
using Moq.AutoMock;
using Kaleido.Modules.Services.Grpc.Currencies.GetAll;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Common.Builders;
using System.Linq.Expressions;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.GetAll
{
    public class GetAllManagerTests
    {
        private readonly AutoMocker _mocker;
        private readonly GetAllManager _sut;

        public GetAllManagerTests()
        {
            _mocker = new AutoMocker();
            _sut = _mocker.CreateInstance<GetAllManager>();

            var currencyEntities = new List<EntityLifeCycleResult<CurrencyEntity, BaseRevisionEntity>>
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
                .ReturnsAsync(currencyEntities);
        }

        [Fact]
        public async Task GetAllAsync_ShouldCallRepositoryGetAllActiveAsync()
        {
            // Act
            await _sut.GetAllAsync();

            // Assert
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity>>()
                .Verify(r => r.FindAllAsync(
                    It.IsAny<Expression<Func<CurrencyEntity, bool>>>(),
                    It.IsAny<Expression<Func<BaseRevisionEntity, bool>>>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyListWhenNoCurrencies()
        {
            // Arrange
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity>>()
                .Setup(r => r.FindAllAsync(
                    It.IsAny<Expression<Func<CurrencyEntity, bool>>>(),
                    It.IsAny<Expression<Func<BaseRevisionEntity, bool>>>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EntityLifeCycleResult<CurrencyEntity, BaseRevisionEntity>>());

            // Act
            var result = await _sut.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_ShouldPassCancellationTokenToHandler()
        {
            // Arrange
            var cancellationToken = new CancellationToken();

            // Act
            await _sut.GetAllAsync(cancellationToken);

            // Assert
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity>>()
                .Verify(r => r.FindAllAsync(
                    It.IsAny<Expression<Func<CurrencyEntity, bool>>>(),
                    It.IsAny<Expression<Func<BaseRevisionEntity, bool>>>(),
                    It.IsAny<Guid?>(),
                    cancellationToken), Times.Once);
        }
    }
}

