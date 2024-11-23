using Moq;
using Moq.AutoMock;
using Kaleido.Modules.Services.Grpc.Currencies.Delete;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using AutoMapper;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Mappers;
using Grpc.Core;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Common.Builders;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Constants;
using Kaleido.Common.Services.Grpc.Exceptions;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.Delete
{
    public class DeleteManagerTests
    {
        private readonly AutoMocker _mocker;
        private readonly DeleteManager _sut;

        public DeleteManagerTests()
        {
            _mocker = new AutoMocker();
            _sut = _mocker.CreateInstance<DeleteManager>();

            var currencyEntity = new EntityLifeCycleResult<CurrencyEntity, BaseRevisionEntity>
            {
                Entity = new CurrencyEntityBuilder().Build(),
                Revision = new CurrencyRevisionBuilder().WithKey(Guid.NewGuid()).Build()
            };

            _mocker.Use(() =>
            {
                var mapper = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<CurrencyMappingProfile>();
                });
                return mapper.CreateMapper();
            });

            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity>>()
                .Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<BaseRevisionEntity>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(currencyEntity);
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallRepositoryDeleteAsync()
        {
            // Arrange
            var key = Guid.NewGuid();

            // Act
            await _sut.DeleteAsync(key.ToString());

            // Assert
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity>>()
                .Verify(r => r.DeleteAsync(key, It.IsAny<BaseRevisionEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnDeletedEntity()
        {
            // Arrange
            var key = Guid.NewGuid();

            // Act
            var result = await _sut.DeleteAsync(key.ToString());

            // Assert
            Assert.NotNull(result.Currency);
            Assert.Equal(ManagerResponseState.Success, result.State);
        }

        [Fact]
        public async Task DeleteAsync_WhenRepositoryThrowsRevisionNotFoundException_ShouldReturnNotFound()
        {
            // Arrange
            var key = Guid.NewGuid();
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity>>()
                .Setup(r => r.DeleteAsync(key, It.IsAny<BaseRevisionEntity>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RevisionNotFoundException("Revision not found"));

            // Act
            var result = await _sut.DeleteAsync(key.ToString());

            // Assert
            Assert.Null(result.Currency);
            Assert.Equal(ManagerResponseState.NotFound, result.State);
        }

    }
}

