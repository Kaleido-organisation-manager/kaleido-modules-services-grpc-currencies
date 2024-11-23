using Moq;
using Moq.AutoMock;
using Grpc.Core;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.GetAllFiltered;
using Kaleido.Common.Services.Grpc.Exceptions;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;
using AutoMapper;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Mappers;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Validators;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Common.Builders;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.GetAllFiltered
{
    public class GetAllFilteredHandlerTests
    {
        private readonly AutoMocker _mocker;
        private readonly GetAllFilteredHandler _sut;

        public GetAllFilteredHandlerTests()
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
            _mocker.Use(new NameValidator());

            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CurrencyMappingProfile>();
            });

            _mocker.Use(mapper.CreateMapper());

            _mocker.GetMock<IGetAllFilteredManager>()
                .Setup(m => m.GetAllByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(currencies.Select(c => ManagerResponse.Success(c)));

            _sut = _mocker.CreateInstance<GetAllFilteredHandler>();
        }

        [Fact]
        public async Task HandleAsync_ValidRequest_ReturnsGetAllCurrenciesByNameResponse()
        {
            // Arrange
            var validRequest = new GetAllCurrenciesFilteredRequestBuilder().Build();

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
            var validRequest = new GetAllCurrenciesFilteredRequestBuilder().Build();

            // Act
            await _sut.HandleAsync(validRequest);

            // Assert
            _mocker.GetMock<IGetAllFilteredManager>()
                .Verify(m => m.GetAllByNameAsync(validRequest.Name, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_ValidationFails_ThrowsValidationException()
        {
            // Arrange
            var invalidRequest = new GetAllCurrenciesFilteredRequestBuilder().WithName("").Build();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<RpcException>(() => _sut.HandleAsync(invalidRequest));
            Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
        }

        [Fact]
        public async Task HandleAsync_ManagerThrowsException_ThrowsRpcException()
        {
            // Arrange
            var validRequest = new GetAllCurrenciesFilteredRequestBuilder().Build();

            _mocker.GetMock<IGetAllFilteredManager>()
                .Setup(m => m.GetAllByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<RpcException>(() => _sut.HandleAsync(validRequest));
            Assert.Equal(StatusCode.Internal, exception.Status.StatusCode);
        }
    }
}

