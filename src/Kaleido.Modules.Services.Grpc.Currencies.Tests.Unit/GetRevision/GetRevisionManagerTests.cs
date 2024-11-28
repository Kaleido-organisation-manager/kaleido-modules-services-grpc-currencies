using Moq;
using Moq.AutoMock;
using Kaleido.Modules.Services.Grpc.Currencies.GetRevision;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.Builders;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Constants;
using System.Linq.Expressions;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.GetRevision
{
    public class GetRevisionManagerTests
    {
        private readonly AutoMocker _mocker;
        private readonly GetRevisionManager _sut;
        private readonly EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity> _currencyEntity;
        private readonly DateTime _revisionTimestamp;

        public GetRevisionManagerTests()
        {
            _mocker = new AutoMocker();
            _sut = _mocker.CreateInstance<GetRevisionManager>();
            _revisionTimestamp = DateTime.UtcNow;

            _currencyEntity = new EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity>
            {
                Entity = new CurrencyEntityBuilder().Build(),
                Revision = new CurrencyRevisionBuilder()
                    .WithKey(Guid.NewGuid())
                    .WithRevision(1)
                    .WithCreatedAt(_revisionTimestamp)
                    .Build()
            };

            // Happy path setup for currency
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity>>()
                .Setup(r => r.GetHistoricAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid key, DateTime createdAt, CancellationToken cancellationToken) =>
                {
                    _currencyEntity.Revision.Key = key;
                    _currencyEntity.Revision.CreatedAt = createdAt;
                    return _currencyEntity;
                });

            // Happy path setup for denominations
            _mocker.GetMock<IEntityLifecycleHandler<DenominationEntity, DenominationRevisionEntity>>()
                .Setup(r => r.FindAllAsync(
                    It.IsAny<Expression<Func<DenominationEntity, bool>>>(),
                    It.IsAny<Expression<Func<DenominationRevisionEntity, bool>>>(),
                    null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>>());
        }

        [Fact]
        public async Task GetRevisionAsync_ShouldCallRepositoryWithCorrectParameters()
        {
            // Arrange
            var key = Guid.NewGuid();
            var createdAt = DateTime.UtcNow;

            // Act
            await _sut.GetRevisionAsync(key, createdAt);

            // Assert
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity>>()
                .Verify(r => r.GetHistoricAsync(key, createdAt, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetRevisionAsync_ShouldReturnMappedCurrencyRevision()
        {
            // Arrange
            var key = Guid.NewGuid();
            var createdAt = DateTime.UtcNow;

            // Act
            var result = await _sut.GetRevisionAsync(key, createdAt);

            // Assert
            Assert.Equal(ManagerResponseState.Success, result.State);
            Assert.NotNull(result.Currency);
            Assert.NotNull(result.Denominations);
        }

        [Fact]
        public async Task GetRevisionAsync_WhenRepositoryReturnsNull_ShouldReturnNotFound()
        {
            // Arrange
            var key = Guid.NewGuid();
            var createdAt = DateTime.UtcNow;

            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity>>()
                .Setup(r => r.GetHistoricAsync(key, createdAt, It.IsAny<CancellationToken>()))
                .ReturnsAsync((EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity>)null!);

            // Act
            var result = await _sut.GetRevisionAsync(key, createdAt);

            // Assert
            Assert.Equal(ManagerResponseState.NotFound, result.State);
        }

        [Fact]
        public async Task GetRevisionAsync_ShouldFetchDenominationsForSameTimestamp()
        {
            // Arrange
            var key = Guid.NewGuid();
            var createdAt = DateTime.UtcNow;

            // Act
            await _sut.GetRevisionAsync(key, createdAt);

            // Assert
            _mocker.GetMock<IEntityLifecycleHandler<DenominationEntity, DenominationRevisionEntity>>()
                .Verify(r => r.FindAllAsync(
                    It.Is<Expression<Func<DenominationEntity, bool>>>(expr => expr.ToString().Contains("CurrencyKey")),
                    It.Is<Expression<Func<DenominationRevisionEntity, bool>>>(expr => expr.ToString().Contains("CreatedAt")),
                    null,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetRevisionAsync_ShouldPassCancellationTokenToRepositories()
        {
            // Arrange
            var key = Guid.NewGuid();
            var createdAt = DateTime.UtcNow;
            var cancellationToken = new CancellationToken();

            // Act
            await _sut.GetRevisionAsync(key, createdAt, cancellationToken);

            // Assert
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity>>()
                .Verify(r => r.GetHistoricAsync(key, createdAt, cancellationToken), Times.Once);

            _mocker.GetMock<IEntityLifecycleHandler<DenominationEntity, DenominationRevisionEntity>>()
                .Verify(r => r.FindAllAsync(
                    It.IsAny<Expression<Func<DenominationEntity, bool>>>(),
                    It.IsAny<Expression<Func<DenominationRevisionEntity, bool>>>(),
                    null,
                    cancellationToken),
                Times.Once);
        }

        [Fact]
        public async Task GetRevisionAsync_ShouldReturnDenominationsWithCurrencyRevision()
        {
            // Arrange
            var key = Guid.NewGuid();
            var createdAt = DateTime.UtcNow;
            var denominations = new List<EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>>
            {
                new()
                {
                    Entity = new DenominationEntityBuilder().Build(),
                    Revision = new DenominationRevisionBuilder()
                        .WithKey(Guid.NewGuid())
                        .WithRevision(1)
                        .WithCreatedAt(_revisionTimestamp)
                        .Build()
                }
            };

            _mocker.GetMock<IEntityLifecycleHandler<DenominationEntity, DenominationRevisionEntity>>()
                .Setup(r => r.FindAllAsync(
                    It.IsAny<Expression<Func<DenominationEntity, bool>>>(),
                    It.IsAny<Expression<Func<DenominationRevisionEntity, bool>>>(),
                    null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(denominations);

            // Act
            var result = await _sut.GetRevisionAsync(key, createdAt);

            // Assert
            Assert.Equal(ManagerResponseState.Success, result.State);
            Assert.NotNull(result.Currency);
            Assert.Single(result.Denominations!);
        }
    }
}

