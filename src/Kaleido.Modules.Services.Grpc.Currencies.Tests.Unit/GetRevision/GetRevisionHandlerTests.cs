using Moq;
using Moq.AutoMock;
using Grpc.Core;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.GetRevision;
using AutoMapper;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Mappers;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Validators;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.Builders;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.GetRevision
{
    public class GetRevisionHandlerTests
    {
        private readonly AutoMocker _mocker;
        private readonly GetRevisionHandler _sut;

        public GetRevisionHandlerTests()
        {
            _mocker = new AutoMocker();

            var key = Guid.NewGuid();
            var validRevision = new EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity>
            {
                Entity = new CurrencyEntityBuilder().Build(),
                Revision = new CurrencyRevisionBuilder().WithKey(key).WithRevision(1).Build()
            };

            var denominations = new List<EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>>()
            {
                new EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>
                {
                    Entity = new DenominationEntityBuilder().Build(),
                    Revision = new DenominationRevisionBuilder().WithKey(key).WithRevision(1).Build()
                }
            };

            // Happy path setup
            _mocker.Use(new KeyValidator());

            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CurrencyMappingProfile>();
            });
            _mocker.Use(mapper.CreateMapper());

            _mocker.GetMock<IGetRevisionManager>()
                .Setup(m => m.GetRevisionAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid key, DateTime createdAt, CancellationToken cancellationToken) =>
                {
                    validRevision.Revision.CreatedAt = createdAt;
                    validRevision.Revision.Key = key;
                    return ManagerResponse.Success(validRevision, denominations);
                }

                );

            _sut = _mocker.CreateInstance<GetRevisionHandler>();
        }

        [Fact]
        public async Task HandleAsync_ValidRequest_ReturnsGetCategoryRevisionResponse()
        {
            // Arrange
            var key = Guid.NewGuid();
            var request = new GetCurrencyRevisionRequestBuilder().WithKey(key.ToString()).Build();

            // Act
            var result = await _sut.HandleAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CurrencyResponse>(result);
            Assert.Equal(key.ToString(), result.Key);
        }

        [Fact]
        public async Task HandleAsync_ValidRequest_CallsManager()
        {
            // Arrange
            var key = Guid.NewGuid();
            var request = new GetCurrencyRevisionRequestBuilder().WithKey(key.ToString()).Build();

            // Act
            await _sut.HandleAsync(request);

            // Assert
            _mocker.GetMock<IGetRevisionManager>()
                .Verify(m => m.GetRevisionAsync(key, request.CreatedAt.ToDateTime(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_ValidationFails_ThrowsValidationException()
        {
            // Arrange
            var invalidRequest = new GetCurrencyRevisionRequestBuilder().WithKey("invalid-key").Build();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<RpcException>(() => _sut.HandleAsync(invalidRequest));
            Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
        }

        [Fact]
        public async Task HandleAsync_ManagerReturnsNull_ThrowsRpcException()
        {
            // Arrange
            var key = Guid.NewGuid();
            var request = new GetCurrencyRevisionRequestBuilder().WithKey(key.ToString()).Build();

            _mocker.GetMock<IGetRevisionManager>()
                .Setup(m => m.GetRevisionAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ManagerResponse.NotFound());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<RpcException>(() => _sut.HandleAsync(request));
            Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
        }

        [Fact]
        public async Task HandleAsync_ManagerThrowsException_ThrowsRpcException()
        {
            // Arrange
            var key = Guid.NewGuid();
            var request = new GetCurrencyRevisionRequestBuilder().WithKey(key.ToString()).Build();

            _mocker.GetMock<IGetRevisionManager>()
                .Setup(m => m.GetRevisionAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<RpcException>(() => _sut.HandleAsync(request));
            Assert.Equal(StatusCode.Internal, exception.Status.StatusCode);
        }
    }
}

