using Moq;
using Moq.AutoMock;
using Kaleido.Modules.Services.Grpc.Currencies.Get;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Common.Builders;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Constants;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.Get
{
    public class GetManagerTests
    {
        private readonly AutoMocker _mocker;
        private readonly GetManager _sut;

        public GetManagerTests()
        {
            _mocker = new AutoMocker();
            _sut = _mocker.CreateInstance<GetManager>();

            var currencyEntity = new EntityLifeCycleResult<CurrencyEntity, BaseRevisionEntity>
            {
                Entity = new CurrencyEntityBuilder().Build(),
                Revision = new CurrencyRevisionBuilder().Build()
            };

            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity>>()
                .Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid key, int? revision, CancellationToken cancellationToken) =>
                {
                    currencyEntity.Revision.Key = key;
                    return currencyEntity;
                });
        }

        [Fact]
        public async Task GetAsync_ShouldCallRepositoryGetActiveAsync()
        {
            // Arrange
            var key = Guid.NewGuid();

            // Act
            await _sut.GetAsync(key.ToString());

            // Assert
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity>>()
                .Verify(r => r.GetAsync(key, It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnMappedCategory()
        {
            // Arrange
            var key = Guid.NewGuid();

            // Act
            var result = await _sut.GetAsync(key.ToString());

            // Assert
            Assert.NotNull(result.Currency);
            Assert.Equal(ManagerResponseState.Success, result.State);
        }

        [Fact]
        public async Task GetAsync_WhenRepositoryReturnsNull_ShouldReturnNull()
        {
            // Arrange
            var key = Guid.NewGuid();
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity>>()
                .Setup(r => r.GetAsync(key, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((EntityLifeCycleResult<CurrencyEntity, BaseRevisionEntity>)null!);

            // Act
            var result = await _sut.GetAsync(key.ToString());

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
            await _sut.GetAsync(key.ToString(), cancellationToken);

            // Assert
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity>>()
                .Verify(r => r.GetAsync(key, It.IsAny<int?>(), cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetAsync_ShouldParseKeyCorrectly()
        {
            // Arrange
            var key = Guid.NewGuid();

            // Act
            await _sut.GetAsync(key.ToString());

            // Assert
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity>>()
                .Verify(r => r.GetAsync(key, It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

