using Moq;
using Moq.AutoMock;
using Kaleido.Modules.Services.Grpc.Currencies.GetAllRevisions;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Common.Builders;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.GetAllRevisions
{
    public class GetAllRevisionsManagerTests
    {
        private readonly AutoMocker _mocker;
        private readonly GetAllRevisionsManager _sut;

        public GetAllRevisionsManagerTests()
        {
            _mocker = new AutoMocker();
            _sut = _mocker.CreateInstance<GetAllRevisionsManager>();

            var revisionKey = Guid.NewGuid();
            var validRevisions = new List<EntityLifeCycleResult<CurrencyEntity, BaseRevisionEntity>>
            {
                new EntityLifeCycleResult<CurrencyEntity, BaseRevisionEntity>
                {
                    Entity = new CurrencyEntityBuilder().Build(),
                    Revision = new CurrencyRevisionBuilder().WithKey(revisionKey).WithRevision(1).Build()
                },
                new EntityLifeCycleResult<CurrencyEntity, BaseRevisionEntity>
                {
                    Entity = new CurrencyEntityBuilder().Build(),
                    Revision = new CurrencyRevisionBuilder().WithKey(revisionKey).WithRevision(2).Build()
                }
            };

            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity>>()
                .Setup(r => r.GetAllAsync(revisionKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validRevisions);
        }

        [Fact]
        public async Task HandleAsync_ShouldCallRepositoryGetAllRevisionsAsync()
        {
            // Arrange
            var key = Guid.NewGuid();

            // Act
            await _sut.GetAllRevisionsAsync(key);

            // Assert
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity>>()
                .Verify(r => r.GetAllAsync(key, It.IsAny<CancellationToken>()), Times.Once);
        }


        [Fact]
        public async Task HandleAsync_ShouldPassCancellationTokenToRepository()
        {
            // Arrange
            var key = Guid.NewGuid();
            var cancellationToken = new CancellationToken();

            // Act
            await _sut.GetAllRevisionsAsync(key, cancellationToken);

            // Assert
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity>>()
                .Verify(r => r.GetAllAsync(key, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_WithEmptyResult_ShouldReturnEmptyList()
        {
            // Arrange
            var key = Guid.NewGuid();
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity>>()
                .Setup(r => r.GetAllAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EntityLifeCycleResult<CurrencyEntity, BaseRevisionEntity>>());

            // Act
            var result = await _sut.GetAllRevisionsAsync(key);

            // Assert
            Assert.Empty(result);
        }
    }
}

