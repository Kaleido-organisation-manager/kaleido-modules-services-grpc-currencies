using Moq;
using Moq.AutoMock;
using Grpc.Core;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.Delete;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Validators;
using AutoMapper;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Mappers;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Common.Builders;
using Kaleido.Common.Services.Grpc.Builders;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.Delete
{
    public class DeleteHandlerTests
    {
        private readonly AutoMocker _mocker;
        private readonly DeleteHandler _sut;

        public DeleteHandlerTests()
        {
            _mocker = new AutoMocker();

            _mocker.Use(new KeyValidator());

            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CurrencyMappingProfile>();
            });
            _mocker.Use(mapper.CreateMapper());

            _mocker.GetMock<IDeleteManager>()
                .Setup(m => m.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid key, CancellationToken ct) =>
                    ManagerResponse.Success(
                        new EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity>
                        {
                            Entity = new CurrencyEntityBuilder().Build(),
                            Revision = new CurrencyRevisionBuilder().WithKey(key).Build()
                        },
                        new List<EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>>
                        {
                            new EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>
                            {
                            Entity = new DenominationEntityBuilder().Build(),
                                Revision = new DenominationRevisionBuilder().WithKey(key).Build()
                            }
                        }
                    ));

            _sut = _mocker.CreateInstance<DeleteHandler>();
        }

        [Fact]
        public async Task HandleAsync_ValidRequest_ReturnsCurrencyResponse()
        {
            // Arrange
            var validRequest = new CurrencyRequestBuilder().Build();

            // Act
            var result = await _sut.HandleAsync(validRequest);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CurrencyResponse>(result);
            Assert.Equal(validRequest.Key, result.Key);
        }

        [Fact]
        public async Task HandleAsync_ManagerReturnsNull_ThrowsRpcException()
        {
            // Arrange
            var validRequest = new CurrencyRequestBuilder().Build();
            _mocker.GetMock<IDeleteManager>()
                .Setup(m => m.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ManagerResponse.NotFound());
            // Act & Assert
            var exception = await Assert.ThrowsAsync<RpcException>(() => _sut.HandleAsync(validRequest));
            Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
        }

        [Fact]
        public async Task HandleAsync_ManagerThrowsException_ThrowsRpcException()
        {
            // Arrange
            var validRequest = new CurrencyRequestBuilder().Build();
            _mocker.GetMock<IDeleteManager>()
                .Setup(m => m.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<RpcException>(() => _sut.HandleAsync(validRequest));
            Assert.Equal(StatusCode.Internal, exception.Status.StatusCode);
        }

        [Fact]
        public async Task HandleAsync_InvalidKeyFormat_ThrowsValidationException()
        {
            // Arrange
            var invalidRequest = new CurrencyRequestBuilder().WithKey("invalid-key-format").Build();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<RpcException>(() => _sut.HandleAsync(invalidRequest));
            Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
        }

        [Fact]
        public async Task HandleAsync_EmptyKey_ThrowsValidationException()
        {
            // Arrange
            var emptyKeyRequest = new CurrencyRequestBuilder().WithKey(string.Empty).Build();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<RpcException>(() => _sut.HandleAsync(emptyKeyRequest));
            Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
        }
    }
}

