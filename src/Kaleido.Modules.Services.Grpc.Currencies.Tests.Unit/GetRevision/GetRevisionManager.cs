using Moq;
using Moq.AutoMock;
using Kaleido.Modules.Services.Grpc.Currencies.GetRevision;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Common.Builders;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Constants;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.GetRevision
{
    public class GetRevisionManagerTests
    {
        private readonly AutoMocker _mocker;
        private readonly GetRevisionManager _sut;

        public GetRevisionManagerTests()
        {
            _mocker = new AutoMocker();
            _sut = _mocker.CreateInstance<GetRevisionManager>();

            var currencyEntity = new EntityLifeCycleResult<CurrencyEntity, BaseRevisionEntity>
            {
                Entity = new CurrencyEntityBuilder().Build(),
                Revision = new CurrencyRevisionBuilder().WithKey(Guid.NewGuid()).WithRevision(1).Build()
            };

            // Happy path setup
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity>>()
                .Setup(r => r.GetHistoricAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid key, DateTime createdAt, CancellationToken cancellationToken) =>
                {
                    currencyEntity.Revision.Key = key;
                    currencyEntity.Revision.CreatedAt = createdAt;
                    return currencyEntity;
                });
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
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity>>()
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
        }

        [Fact]
        public async Task GetRevisionAsync_WhenRepositoryReturnsNull_ShouldReturnNotFound()
        {
            // Arrange
            var key = Guid.NewGuid();
            var createdAt = DateTime.UtcNow;

            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity>>()
                .Setup(r => r.GetHistoricAsync(key, createdAt, It.IsAny<CancellationToken>()))
                .ReturnsAsync((EntityLifeCycleResult<CurrencyEntity, BaseRevisionEntity>)null!);

            // Act
            var result = await _sut.GetRevisionAsync(key, createdAt);

            // Assert
            Assert.Equal(ManagerResponseState.NotFound, result.State);
        }

        [Fact]
        public async Task GetRevisionAsync_ShouldPassCancellationTokenToRepository()
        {
            // Arrange
            var key = Guid.NewGuid();
            var createdAt = DateTime.UtcNow;
            var cancellationToken = new CancellationToken();

            // Act
            await _sut.GetRevisionAsync(key, createdAt, cancellationToken);

            // Assert
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity>>()
                .Verify(r => r.GetHistoricAsync(key, createdAt, cancellationToken), Times.Once);
        }
    }
}

