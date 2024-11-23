using Moq;
using Moq.AutoMock;
using Grpc.Core;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.GetAll;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;
using AutoMapper;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Mappers;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Common.Builders;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.GetAll
{
    public class GetAllHandlerTests
    {
        private readonly AutoMocker _mocker;
        private readonly GetAllHandler _sut;

        public GetAllHandlerTests()
        {
            _mocker = new AutoMocker();

            var currencies = new List<EntityLifeCycleResult<CurrencyEntity, BaseRevisionEntity>>
            {
                new EntityLifeCycleResult<CurrencyEntity, BaseRevisionEntity>
                {
                    Entity = new CurrencyEntityBuilder().Build(),
                    Revision = new CurrencyRevisionBuilder().WithKey(Guid.NewGuid()).Build()
                },
                new EntityLifeCycleResult<CurrencyEntity, BaseRevisionEntity>
                {
                    Entity = new CurrencyEntityBuilder().WithName("Dollar").WithCode("USD").WithSymbol("$").Build(),
                    Revision = new CurrencyRevisionBuilder().WithKey(Guid.NewGuid()).Build()
                }
            };

            // Happy path setup
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CurrencyMappingProfile>();
            });
            _mocker.Use(mapper.CreateMapper());


            _mocker.GetMock<IGetAllManager>()
                .Setup(m => m.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(currencies.Select(c => ManagerResponse.Success(c)));

            _sut = _mocker.CreateInstance<GetAllHandler>();
        }

        [Fact]
        public async Task HandleAsync_ValidRequest_ReturnsGetAllCurrenciesResponse()
        {
            var validRequest = new EmptyRequest();

            // Act
            var result = await _sut.HandleAsync(validRequest);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CurrencyListResponse>(result);
            Assert.NotEmpty(result.Currencies);
        }

        [Fact]
        public async Task HandleAsync_ValidRequest_CallsManager()
        {
            // Arrange
            var validRequest = new EmptyRequest();

            // Act
            await _sut.HandleAsync(validRequest);

            // Assert
            _mocker.GetMock<IGetAllManager>()
                .Verify(m => m.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_ManagerThrowsException_ThrowsRpcException()
        {
            // Arrange
            var validRequest = new EmptyRequest();

            _mocker.GetMock<IGetAllManager>()
                .Setup(m => m.GetAllAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<RpcException>(() => _sut.HandleAsync(validRequest));
            Assert.Equal(StatusCode.Internal, exception.Status.StatusCode);
        }
    }
}

