using Moq;
using Moq.AutoMock;
using Kaleido.Modules.Services.Grpc.Currencies.GetAllRevisions;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Common.Builders;
using System.Linq.Expressions;
using Kaleido.Common.Services.Grpc.Constants;
using AutoMapper;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Mappers;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.GetAllRevisions
{
    public class GetAllRevisionsManagerTests
    {
        private readonly AutoMocker _mocker;
        private readonly GetAllRevisionsManager _sut;

        public GetAllRevisionsManagerTests()
        {
            _mocker = new AutoMocker();

            var revisionKey = Guid.NewGuid();
            var validRevisions = new List<EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity>>
            {
                new EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity>
                {
                    Entity = new CurrencyEntityBuilder().Build(),
                    Revision = new CurrencyRevisionBuilder().WithKey(revisionKey).WithRevision(1).Build()
                },
                new EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity>
                {
                    Entity = new CurrencyEntityBuilder().Build(),
                    Revision = new CurrencyRevisionBuilder().WithKey(revisionKey).WithRevision(2).Build()
                }
            };

            var denominations = new List<EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>>
            {
                new EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>
                {
                    Entity = new DenominationEntityBuilder().Build(),
                    Revision = new DenominationRevisionBuilder().WithKey(revisionKey).WithRevision(1).Build()
                }
            };

            _mocker.Use(new MapperConfiguration(cfg => cfg.AddProfile<CurrencyMappingProfile>()).CreateMapper());

            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity>>()
                .Setup(r => r.GetAllAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validRevisions);

            _mocker.GetMock<IEntityLifecycleHandler<DenominationEntity, DenominationRevisionEntity>>()
                .Setup(r => r.FindAllAsync(
                    It.IsAny<Expression<Func<DenominationEntity, bool>>>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(denominations);

            _sut = _mocker.CreateInstance<GetAllRevisionsManager>();
        }

        [Fact]
        public async Task HandleAsync_ShouldCallRepositoryGetAllRevisionsAsync()
        {
            // Arrange
            var key = Guid.NewGuid();

            // Act
            await _sut.GetAllRevisionsAsync(key);

            // Assert
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity>>()
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
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity>>()
                .Verify(r => r.GetAllAsync(key, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_WithEmptyResult_ShouldReturnEmptyList()
        {
            // Arrange
            var key = Guid.NewGuid();
            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity>>()
                .Setup(r => r.GetAllAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity>>());

            // Act
            var result = await _sut.GetAllRevisionsAsync(key);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task HandleAsync_WithTimeOrderedRevisions_ShouldReturnRevisionsInDescendingOrder()
        {
            // Arrange
            var key = Guid.NewGuid();
            var now = DateTime.UtcNow;
            var revisions = new List<EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity>>
            {
                new()
                {
                    Entity = new CurrencyEntityBuilder().Build(),
                    Revision = new CurrencyRevisionBuilder()
                        .WithKey(key)
                        .WithRevision(1)
                        .WithCreatedAt(now.AddHours(-2))
                        .Build()
                },
                new()
                {
                    Entity = new CurrencyEntityBuilder().Build(),
                    Revision = new CurrencyRevisionBuilder()
                        .WithKey(key)
                        .WithRevision(2)
                        .WithCreatedAt(now)
                        .Build()
                }
            };

            var denominations = new List<EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>>
            {
                new()
                {
                    Entity = new DenominationEntityBuilder().Build(),
                    Revision = new DenominationRevisionBuilder().WithKey(key).WithRevision(1).WithCreatedAt(now.AddHours(-2)).Build()
                }
            };

            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity>>()
                .Setup(r => r.GetAllAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(revisions);

            _mocker.GetMock<IEntityLifecycleHandler<DenominationEntity, DenominationRevisionEntity>>()
                .Setup(r => r.FindAllAsync(It.IsAny<Expression<Func<DenominationEntity, bool>>>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(denominations);

            // Act
            var result = await _sut.GetAllRevisionsAsync(key);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Equal(2, result.ToList()[0].Currency!.Revision.Revision);
            Assert.Equal(1, result.ToList()[1].Currency!.Revision.Revision);
        }

        [Fact]
        public async Task HandleAsync_WithUnmodifiedRevisions_ShouldMarkRevisionsAsUnmodified()
        {
            // Arrange
            var key = Guid.NewGuid();
            var now = DateTime.UtcNow;
            var revisions = new List<EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity>>
            {
        new()
        {
            Entity = new CurrencyEntityBuilder().Build(),
            Revision = new CurrencyRevisionBuilder()
                .WithKey(key)
                .WithRevision(1)
                .WithCreatedAt(now)
                .Build()
        },
        new()
        {
            Entity = new CurrencyEntityBuilder().Build(),
            Revision = new CurrencyRevisionBuilder()
                .WithKey(key)
                .WithRevision(1)
                .WithCreatedAt(now.AddMinutes(-5))
                .Build()
        }
    };

            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity>>()
                .Setup(r => r.GetAllAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(revisions);

            // Act
            var result = (await _sut.GetAllRevisionsAsync(key)).ToList();

            // Assert
            Assert.Equal(RevisionAction.Unmodified, result[1].Currency!.Revision.Action);
        }

        [Fact]
        public async Task HandleAsync_WithDeletedDenominations_ShouldNotIncludeDeletedDenominationsInSubsequentRevisions()
        {
            // Arrange
            var key = Guid.NewGuid();
            var denominationKey = Guid.NewGuid();
            var now = DateTime.UtcNow;

            var currencies = new List<EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity>>
            {
                new()
                {
                    Entity = new CurrencyEntityBuilder().Build(),
                    Revision = new CurrencyRevisionBuilder()
                        .WithKey(key)
                        .WithRevision(1)
                        .WithCreatedAt(now.AddHours(-2))
                        .Build()
                }
            };

            var denominations = new List<EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>>
            {
                new()
                {
                    Entity = new DenominationEntityBuilder().Build(),
                    Revision = new DenominationRevisionBuilder()
                        .WithKey(denominationKey)
                        .WithRevision(1)
                        .WithCreatedAt(now.AddHours(-1))
                        .WithAction(RevisionAction.Deleted)
                        .Build()
                },
                new()
                {
                    Entity = new DenominationEntityBuilder().Build(),
                    Revision = new DenominationRevisionBuilder()
                        .WithKey(denominationKey)
                        .WithRevision(2)
                        .WithCreatedAt(now)
                        .Build()
                }
            };

            _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity>>()
                .Setup(r => r.GetAllAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(currencies);

            _mocker.GetMock<IEntityLifecycleHandler<DenominationEntity, DenominationRevisionEntity>>()
                .Setup(r => r.FindAllAsync(
                    It.IsAny<Expression<Func<DenominationEntity, bool>>>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(denominations);

            // Act
            var result = await _sut.GetAllRevisionsAsync(key);

            // Assert
            Assert.Equal(3, result.Count());
            Assert.DoesNotContain(result.ToList()[2].Denominations!, d => d.Key == denominationKey);
        }
    }
}

