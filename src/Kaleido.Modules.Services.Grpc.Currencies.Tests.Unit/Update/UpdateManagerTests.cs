using Moq;
using Moq.AutoMock;
using Kaleido.Modules.Services.Grpc.Currencies.Update;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Exceptions;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Common.Builders;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Constants;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.Update
{
    public class UpdateManagerTests
    {
        private readonly AutoMocker _mocker;
        private readonly UpdateManager _sut;

        public UpdateManagerTests()
        {
            _mocker = new AutoMocker();

            var currencyEntity = new CurrencyEntityBuilder().Build();


            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity>>()
                .Setup(handler => handler.UpdateAsync(It.IsAny<Guid>(), It.IsAny<CurrencyEntity>(), It.IsAny<BaseRevisionEntity>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid key, CurrencyEntity entity, BaseRevisionEntity revision, CancellationToken cancellationToken) =>
                {
                    return new EntityLifeCycleResult<CurrencyEntity, BaseRevisionEntity>
                    {
                        Entity = entity,
                        Revision = revision ?? new CurrencyRevisionBuilder().WithKey(key).WithRevision(1).Build()
                    };
                });

            _sut = _mocker.CreateInstance<UpdateManager>();
        }

        [Fact]
        public async Task UpdateAsync_ValidCurrency_ReturnsUpdatedEntity()
        {
            // Arrange
            var key = Guid.NewGuid();
            var updatedEntity = new CurrencyEntityBuilder().Build();

            // Act
            var result = await _sut.UpdateAsync(key, updatedEntity);

            // Assert
            Assert.NotNull(result.Currency);
            Assert.Equal(ManagerResponseState.Success, result.State);
        }


        [Fact]
        public async Task UpdateAsync_ManagerThrowsRevisionNotFoundException_ReturnsNotFound()
        {
            // Arrange
            var key = Guid.NewGuid();
            var updatedEntity = new CurrencyEntityBuilder().Build();

            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity>>()
                .Setup(handler => handler.UpdateAsync(It.IsAny<Guid>(), It.IsAny<CurrencyEntity>(), It.IsAny<BaseRevisionEntity>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RevisionNotFoundException("Test exception"));

            // Act
            var result = await _sut.UpdateAsync(key, updatedEntity);

            // Assert
            Assert.Equal(ManagerResponseState.NotFound, result.State);
        }

    }
}

