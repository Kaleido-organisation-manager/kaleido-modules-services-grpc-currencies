using Moq;
using Moq.AutoMock;
using Grpc.Core;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.Create;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Validators;
using AutoMapper;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Mappers;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Common.Builders;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.Create
{
    public class CreateHandlerTests
    {
        private readonly AutoMocker _mocker;
        private readonly CreateHandler _sut;

        public CreateHandlerTests()
        {
            _mocker = new AutoMocker();

            // Happy path setup
            _mocker.Use(new CurrencyValidator());

            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CurrencyMappingProfile>();
            });
            _mocker.Use(mapper.CreateMapper());

            _mocker.GetMock<ICreateManager>()
                .Setup(m => m.CreateAsync(It.IsAny<CurrencyEntity>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    ManagerResponse.Success(new EntityLifeCycleResult<CurrencyEntity, BaseRevisionEntity>
                    {
                        Entity = new CurrencyEntityBuilder().Build(),
                        Revision = new BaseRevisionEntity()
                    })
                );

            _sut = _mocker.CreateInstance<CreateHandler>();
        }

        [Fact]
        public async Task HandleAsync_ValidRequest_ReturnsCurrencyResponse()
        {
            // Arrange
            var validRequest = new CurrencyBuilder().Build();

            // Act
            var result = await _sut.HandleAsync(validRequest);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CurrencyResponse>(result);
        }

        [Fact]
        public async Task HandleAsync_ValidRequest_CallsValidatorAndManager()
        {
            // Arrange
            var validRequest = new CurrencyBuilder().Build();

            // Act
            await _sut.HandleAsync(validRequest);

            // Assert

            _mocker.GetMock<ICreateManager>()
                .Verify(m => m.CreateAsync(It.IsAny<CurrencyEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_ValidationFails_ThrowsRpcException()
        {
            // Arrange
            var invalidRequest = new CurrencyBuilder().WithName("").Build();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<RpcException>(() => _sut.HandleAsync(invalidRequest));
            Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
        }

        [Fact]
        public async Task HandleAsync_ManagerThrowsException_ThrowsRpcException()
        {
            // Arrange
            var validRequest = new CurrencyBuilder().Build();

            _mocker.GetMock<ICreateManager>()
                .Setup(m => m.CreateAsync(It.IsAny<CurrencyEntity>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<RpcException>(() => _sut.HandleAsync(validRequest));
            Assert.Equal(StatusCode.Internal, exception.Status.StatusCode);
        }
    }
}

