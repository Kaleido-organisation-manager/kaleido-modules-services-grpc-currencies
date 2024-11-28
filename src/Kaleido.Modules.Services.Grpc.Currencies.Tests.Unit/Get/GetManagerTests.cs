using Moq;
using Moq.AutoMock;
using Kaleido.Modules.Services.Grpc.Currencies.Get;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.Builders;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Constants;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;
using System.Linq.Expressions;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.Get
{
    public class GetManagerTests
    {
        private readonly AutoMocker _mocker;
        private readonly GetManager _sut;

        public GetManagerTests()
        {
            _mocker = new AutoMocker();

            var currencyEntity = new EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity>
            {
                Entity = new CurrencyEntityBuilder().Build(),
                Revision = new CurrencyRevisionBuilder().Build()
            };

            var denominationList = new List<EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>>()
            {
                new EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>
                {
                    Entity = new DenominationEntityBuilder().Build(),
                    Revision = new DenominationRevisionBuilder().Build()
                }
            };

            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity>>()
                .Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid key, int? revision, CancellationToken cancellationToken) =>
                {
                    currencyEntity.Revision.Key = key;
                    return currencyEntity;
                });

            _mocker.GetMock<IEntityLifecycleHandler<DenominationEntity, DenominationRevisionEntity>>()
                .Setup(r => r.FindAllAsync(
                    It.IsAny<Expression<Func<DenominationEntity, bool>>>(),
                    It.IsAny<Expression<Func<DenominationRevisionEntity, bool>>>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(denominationList);

            _sut = _mocker.CreateInstance<GetManager>();
        }

        [Fact]
        public async Task GetAsync_ShouldCallRepositoryGetActiveAsync()
        {
            // Arrange
            var key = Guid.NewGuid();

            // Act
            await _sut.GetAsync(key);

            // Assert
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity>>()
                .Verify(r => r.GetAsync(key, It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnMappedCategory()
        {
            // Arrange
            var key = Guid.NewGuid();

            // Act
            var result = await _sut.GetAsync(key);

            // Assert
            Assert.NotNull(result.Currency);
            Assert.Equal(ManagerResponseState.Success, result.State);
        }

        [Fact]
        public async Task GetAsync_WhenRepositoryReturnsNull_ShouldReturnNull()
        {
            // Arrange
            var key = Guid.NewGuid();
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity>>()
                .Setup(r => r.GetAsync(key, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity>)null!);

            // Act
            var result = await _sut.GetAsync(key);

            // Assert
            Assert.Null(result.Currency);
            Assert.Equal(ManagerResponseState.NotFound, result.State);
        }

        [Fact]
        public async Task GetAsync_ShouldPassCancellationTokenToRepository()
        {
            // Arrange
            var key = Guid.NewGuid();
            var cancellationToken = new CancellationToken();

            // Act
            await _sut.GetAsync(key, cancellationToken);

            // Assert
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity>>()
                .Verify(r => r.GetAsync(key, It.IsAny<int?>(), cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetAsync_ShouldParseKeyCorrectly()
        {
            // Arrange
            var key = Guid.NewGuid();

            // Act
            await _sut.GetAsync(key);

            // Assert
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity>>()
                .Verify(r => r.GetAsync(key, It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnDenominations()
        {
            // Arrange
            var key = Guid.NewGuid();

            // Act
            var result = await _sut.GetAsync(key);

            // Assert
            Assert.NotNull(result.Denominations);
            Assert.NotEmpty(result.Denominations);
        }

        [Fact]
        public async Task GetAsync_ShouldCallFindAllAsync()
        {
            // Arrange
            var key = Guid.NewGuid();

            // Act
            await _sut.GetAsync(key);

            // Assert
            _mocker.GetMock<IEntityLifecycleHandler<DenominationEntity, DenominationRevisionEntity>>()
                .Verify(r => r.FindAllAsync(
                    It.IsAny<Expression<Func<DenominationEntity, bool>>>(),
                    It.IsAny<Expression<Func<DenominationRevisionEntity, bool>>>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}

