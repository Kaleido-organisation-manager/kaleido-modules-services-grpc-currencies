using Moq;
using Moq.AutoMock;
using Kaleido.Modules.Services.Grpc.Currencies.Update;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Exceptions;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Common.Builders;
using Kaleido.Common.Services.Grpc.Constants;
using AutoMapper;
using System.Linq.Expressions;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Constants;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.Update
{
    public class UpdateManagerTests
    {
        private readonly AutoMocker _mocker;
        private readonly UpdateManager _sut;
        private readonly IMapper _mapper;

        public UpdateManagerTests()
        {
            _mocker = new AutoMocker();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>,
                    EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>>();
            });
            _mapper = mapperConfig.CreateMapper();
            _mocker.Use(_mapper);

            // Setup currency lifecycle handler
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity>>()
                .Setup(handler => handler.UpdateAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CurrencyEntity>(),
                    It.IsAny<CurrencyRevisionEntity>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid key, CurrencyEntity entity, CurrencyRevisionEntity revision, CancellationToken cancellationToken) =>
                {
                    return new EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity>
                    {
                        Entity = entity,
                        Revision = revision ?? new CurrencyRevisionBuilder().WithKey(key).WithRevision(1).Build(),
                    };
                });

            // Setup denomination lifecycle handler
            _mocker.GetMock<IEntityLifecycleHandler<DenominationEntity, DenominationRevisionEntity>>()
                .Setup(handler => handler.FindAllAsync(
                    It.IsAny<Expression<Func<DenominationEntity, bool>>>(),
                    It.IsAny<Expression<Func<DenominationRevisionEntity, bool>>>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>>());

            _sut = _mocker.CreateInstance<UpdateManager>();
        }

        [Fact]
        public async Task UpdateAsync_ValidCurrency_ReturnsUpdatedEntity()
        {
            // Arrange
            var key = Guid.NewGuid();
            var updatedEntity = new CurrencyEntityBuilder().Build();
            var denominations = new List<DenominationEntity>
            {
                new DenominationEntity { Value = 100, CurrencyKey = key }
            };

            // Act
            var result = await _sut.UpdateAsync(key, updatedEntity, denominations);

            // Assert
            Assert.Equal(ManagerResponseState.Success, result.State);
            Assert.NotNull(result.Currency);
        }

        [Fact]
        public async Task UpdateAsync_WithNewDenominations_CreatesDenominations()
        {
            // Arrange
            var key = Guid.NewGuid();
            var updatedEntity = new CurrencyEntityBuilder().Build();
            var newDenominations = new List<DenominationEntity>
            {
                new DenominationEntity { Value = 100, CurrencyKey = key }
            };

            // Act
            await _sut.UpdateAsync(key, updatedEntity, newDenominations);

            // Assert
            _mocker.GetMock<IEntityLifecycleHandler<DenominationEntity, DenominationRevisionEntity>>()
                .Verify(x => x.CreateAsync(
                    It.IsAny<DenominationEntity>(),
                    It.IsAny<DenominationRevisionEntity>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithDeletedDenominations_DeletesDenominations()
        {
            // Arrange
            var key = Guid.NewGuid();
            var existingDenomination = new EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>
            {
                Entity = new DenominationEntity { Value = 100, CurrencyKey = key },
                Revision = new DenominationRevisionEntity { Action = RevisionAction.Created, Key = key },
            };

            _mocker.GetMock<IEntityLifecycleHandler<DenominationEntity, DenominationRevisionEntity>>()
                .Setup(x => x.FindAllAsync(
                    It.IsAny<Expression<Func<DenominationEntity, bool>>>(),
                    It.IsAny<Expression<Func<DenominationRevisionEntity, bool>>>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>> { existingDenomination });

            var updatedEntity = new CurrencyEntityBuilder().Build();
            var newDenominations = new List<DenominationEntity>(); // Empty list means all existing denominations should be deleted

            // Act
            await _sut.UpdateAsync(key, updatedEntity, newDenominations);

            // Assert
            _mocker.GetMock<IEntityLifecycleHandler<DenominationEntity, DenominationRevisionEntity>>()
                .Verify(x => x.DeleteAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<DenominationRevisionEntity>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ManagerThrowsRevisionNotFoundException_ReturnsNotFound()
        {
            // Arrange
            var key = Guid.NewGuid();
            var updatedEntity = new CurrencyEntityBuilder().Build();
            var denominations = new List<DenominationEntity>();

            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity>>()
                .Setup(handler => handler.UpdateAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CurrencyEntity>(),
                    It.IsAny<CurrencyRevisionEntity>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RevisionNotFoundException("Test exception"));

            // Act
            var result = await _sut.UpdateAsync(key, updatedEntity, denominations);

            // Assert
            Assert.Equal(ManagerResponseState.NotFound, result.State);
        }

        [Fact]
        public async Task UpdateAsync_NotModifiedExceptionThrown_GetsExistingEntity()
        {
            // Arrange
            var key = Guid.NewGuid();
            var updatedEntity = new CurrencyEntityBuilder().Build();
            var denominations = new List<DenominationEntity>();

            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity>>()
                .Setup(handler => handler.UpdateAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CurrencyEntity>(),
                    It.IsAny<CurrencyRevisionEntity>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new NotModifiedException("Test exception"));

            // Act
            await _sut.UpdateAsync(key, updatedEntity, denominations);

            // Assert
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity>>()
                .Verify(x => x.GetAsync(
                    key,
                    It.IsAny<int?>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}

