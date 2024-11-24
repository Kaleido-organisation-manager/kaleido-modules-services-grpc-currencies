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
using System.Linq.Expressions;

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

            var currencyEntity = new EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity>
            {
                Entity = new CurrencyEntityBuilder().Build(),
                Revision = new CurrencyRevisionBuilder().WithKey(Guid.NewGuid()).Build()
            };

            var denominationList = new List<EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>>()
            {
                new EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>
                {
                    Entity = new DenominationEntityBuilder().WithValue(2).WithCurrencyKey(currencyEntity.Key).Build(),
                    Revision = new DenominationRevisionBuilder().WithKey(Guid.NewGuid()).Build()
                },
                new EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>
                {
                    Entity = new DenominationEntityBuilder().WithValue(1).WithCurrencyKey(currencyEntity.Key).Build(),
                    Revision = new DenominationRevisionBuilder().WithKey(Guid.NewGuid()).Build()
                }
            };

            _mocker.Use(() =>
            {
                var mapper = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<CurrencyMappingProfile>();
                });
                return mapper.CreateMapper();
            });

            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity>>()
                .Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(currencyEntity);

            _mocker.GetMock<IEntityLifecycleHandler<DenominationEntity, DenominationRevisionEntity>>()
                .Setup(r => r.FindAllAsync(
                    It.IsAny<Expression<Func<DenominationEntity, bool>>>(),
                    It.IsAny<Expression<Func<DenominationRevisionEntity, bool>>>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(denominationList);

            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity>>()
                .Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CurrencyRevisionEntity>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(currencyEntity);

            _mocker.GetMock<IEntityLifecycleHandler<DenominationEntity, DenominationRevisionEntity>>()
                .Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<DenominationRevisionEntity>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid key, DenominationRevisionEntity revision, CancellationToken ct) => denominationList.First(d => d.Key == key));
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallRepositoryDeleteAsync()
        {
            // Arrange
            var key = Guid.NewGuid();

            // Act
            await _sut.DeleteAsync(key);

            // Assert
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity>>()
                .Verify(r => r.DeleteAsync(key, It.IsAny<CurrencyRevisionEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnDeletedEntity()
        {
            // Arrange
            var key = Guid.NewGuid();

            // Act
            var result = await _sut.DeleteAsync(key);

            // Assert
            Assert.NotNull(result.Currency);
            Assert.Equal(ManagerResponseState.Success, result.State);
        }

        [Fact]
        public async Task DeleteAsync_WhenRepositoryThrowsRevisionNotFoundException_ShouldReturnNotFound()
        {
            // Arrange
            var key = Guid.NewGuid();
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity>>()
                .Setup(r => r.GetAsync(key, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RevisionNotFoundException("Revision not found"));

            // Act
            var result = await _sut.DeleteAsync(key);

            // Assert
            Assert.Null(result.Currency);
            Assert.Equal(ManagerResponseState.NotFound, result.State);
        }

    }
}

