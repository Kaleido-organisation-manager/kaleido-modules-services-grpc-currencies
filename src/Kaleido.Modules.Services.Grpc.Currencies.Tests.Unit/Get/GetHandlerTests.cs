using Xunit;
using Moq;
using Moq.AutoMock;
using Grpc.Core;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.Get;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Validators;
using AutoMapper;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Mappers;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.Builders;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.Get
{
    public class GetHandlerTests
    {
        private readonly AutoMocker _mocker;
        private readonly GetHandler _sut;

        public GetHandlerTests()
        {
            _mocker = new AutoMocker();

            var validRequest = new CurrencyRequestBuilder().Build();
            var validCurrency = new EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity>
            {
                Entity = new CurrencyEntityBuilder().Build(),
                Revision = new CurrencyRevisionBuilder().Build()
            };

            var denominations = new List<EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>>()
            {
                new EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>
                {
                    Entity = new DenominationEntityBuilder().Build(),
                    Revision = new DenominationRevisionBuilder().Build()
                }
            };

            _mocker.Use(new KeyValidator());

            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CurrencyMappingProfile>();
            });
            _mocker.Use(mapper.CreateMapper());

            // Happy path setup
            _mocker.GetMock<IGetManager>()
                .Setup(m => m.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ManagerResponse.Success(validCurrency, denominations));

            _sut = _mocker.CreateInstance<GetHandler>();
        }

        [Fact]
        public async Task HandleAsync_ValidRequest_ReturnsGetCategoryResponse()
        {
            // Arrange
            var validRequest = new CurrencyRequestBuilder().Build();

            // Act
            var result = await _sut.HandleAsync(validRequest);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CurrencyResponse>(result);
        }

        [Fact]
        public async Task HandleAsync_ManagerReturnsNull_ThrowsRpcException()
        {
            // Arrange
            var validRequest = new CurrencyRequestBuilder().Build();
            _mocker.GetMock<IGetManager>()
                .Setup(m => m.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
            _mocker.GetMock<IGetManager>()
                .Setup(m => m.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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

