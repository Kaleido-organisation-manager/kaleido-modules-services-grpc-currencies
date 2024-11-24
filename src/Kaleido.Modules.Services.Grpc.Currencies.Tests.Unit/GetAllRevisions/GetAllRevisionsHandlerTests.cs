using Moq;
using Moq.AutoMock;
using Grpc.Core;
using Kaleido.Common.Services.Grpc.Models.Validations;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.GetAllRevisions;
using Kaleido.Common.Services.Grpc.Exceptions;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Validators;
using AutoMapper;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Mappers;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Common.Builders;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.GetAllRevisions
{
    public class GetAllRevisionsHandlerTests
    {
        private readonly AutoMocker _mocker;
        private readonly GetAllRevisionsHandler _sut;

        public GetAllRevisionsHandlerTests()
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

            var denominations = new List<EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>>()
            {
                new EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>
                {
                    Entity = new DenominationEntityBuilder().Build(),
                    Revision = new DenominationRevisionBuilder().Build()
                }
            };

            // Happy path setup
            _mocker.Use(new KeyValidator());

            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CurrencyMappingProfile>();
            });
            _mocker.Use(mapper.CreateMapper());

            _mocker.GetMock<IGetAllRevisionsManager>()
                .Setup(m => m.GetAllRevisionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validRevisions.Select(r => ManagerResponse.Success(r, denominations)));

            _sut = _mocker.CreateInstance<GetAllRevisionsHandler>();
        }

        [Fact]
        public async Task HandleAsync_ValidRequest_ReturnsGetAllCategoryRevisionsResponse()
        {
            // Arrange
            var request = new CurrencyRequestBuilder().Build();

            // Act
            var result = await _sut.HandleAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CurrencyListResponse>(result);
            Assert.NotEmpty(result.Currencies);
        }

        [Fact]
        public async Task HandleAsync_ValidRequest_CallsManager()
        {
            // Arrange
            var key = Guid.NewGuid();
            var request = new CurrencyRequestBuilder().WithKey(key.ToString()).Build();

            // Act
            await _sut.HandleAsync(request);

            // Assert
            _mocker.GetMock<IGetAllRevisionsManager>()
                .Verify(m => m.GetAllRevisionsAsync(key, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_ValidationFails_ThrowsValidationException()
        {
            // Arrange
            var invalidRequest = new CurrencyRequestBuilder().WithKey(string.Empty).Build();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<RpcException>(() => _sut.HandleAsync(invalidRequest));
            Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
        }

        [Fact]
        public async Task HandleAsync_ManagerThrowsException_ThrowsRpcException()
        {
            // Arrange
            var key = Guid.NewGuid();
            var request = new CurrencyRequestBuilder().WithKey(key.ToString()).Build();
            _mocker.GetMock<IGetAllRevisionsManager>()
                .Setup(m => m.GetAllRevisionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<RpcException>(() => _sut.HandleAsync(request));
            Assert.Equal(StatusCode.Internal, exception.Status.StatusCode);
        }

        [Fact]
        public async Task HandleAsync_ManagerReturnsEmptyList_ReturnsEmptyResponse()
        {
            // Arrange
            var key = Guid.NewGuid();
            var request = new CurrencyRequestBuilder().WithKey(key.ToString()).Build();
            _mocker.GetMock<IGetAllRevisionsManager>()
                .Setup(m => m.GetAllRevisionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ManagerResponse>());

            // Act
            var result = await _sut.HandleAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CurrencyListResponse>(result);
            Assert.Empty(result.Currencies);
        }
    }
}

