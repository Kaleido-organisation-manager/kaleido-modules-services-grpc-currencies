using Moq;
using Moq.AutoMock;
using Grpc.Core;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.Update;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Validators;
using AutoMapper;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Mappers;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Common.Builders;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.Update
{
    public class UpdateHandlerTests
    {
        private readonly AutoMocker _mocker;
        private readonly UpdateHandler _sut;
        private readonly EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity> _validRevision;

        public UpdateHandlerTests()
        {
            _mocker = new AutoMocker();
            var key = Guid.NewGuid();

            _validRevision = new EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity>
            {
                Entity = new CurrencyEntityBuilder().Build(),
                Revision = new CurrencyRevisionBuilder()
                    .WithKey(key)
                    .WithRevision(1)
                    .Build()
            };

            // Happy path setup
            _mocker.Use(new KeyValidator());
            _mocker.Use(new CurrencyValidator());

            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CurrencyMappingProfile>();
            });
            _mocker.Use(mapper.CreateMapper());

            _mocker.GetMock<IUpdateManager>()
                .Setup(m => m.UpdateAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CurrencyEntity>(),
                    It.IsAny<IEnumerable<DenominationEntity>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid key, CurrencyEntity entity, IEnumerable<DenominationEntity> denominations, CancellationToken cancellationToken) =>
                {
                    _validRevision.Revision.Key = key;
                    _validRevision.Entity = entity;
                    return ManagerResponse.Success(_validRevision, denominations.Select(d =>
                        new EntityLifeCycleResult<DenominationEntity, DenominationRevisionEntity>
                        {
                            Entity = d,
                            Revision = new DenominationRevisionBuilder().WithKey(Guid.NewGuid()).Build()
                        }));
                });

            _sut = _mocker.CreateInstance<UpdateHandler>();
        }

        [Fact]
        public async Task HandleAsync_ValidRequest_ReturnsUpdateCurrencyResponse()
        {
            // Arrange
            var key = Guid.NewGuid();
            var request = new CurrencyActionRequestBuilder()
                .WithKey(key.ToString())
                .WithCurrency(new CurrencyBuilder()
                    .WithDenominations(new List<Denomination> { new DenominationBuilder().Build() })
                    .Build())
                .Build();

            // Act
            var result = await _sut.HandleAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CurrencyResponse>(result);
            Assert.NotNull(result.Currency);
            Assert.NotNull(result.Currency.Denominations);
        }

        [Fact]
        public async Task HandleAsync_ValidRequest_CallsManagerWithCorrectParameters()
        {
            // Arrange
            var key = Guid.NewGuid();
            var request = new CurrencyActionRequestBuilder()
                .WithKey(key.ToString())
                .WithCurrency(new CurrencyBuilder()
                    .WithDenominations(new List<Denomination> { new DenominationBuilder().Build() })
                    .Build())
                .Build();

            // Act
            await _sut.HandleAsync(request);

            // Assert
            _mocker.GetMock<IUpdateManager>()
                .Verify(m => m.UpdateAsync(
                    key,
                    It.Is<CurrencyEntity>(c => c != null),
                    It.Is<IEnumerable<DenominationEntity>>(d => d != null && d.Any()),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task HandleAsync_ValidationFails_ThrowsValidationException()
        {
            // Arrange
            var invalidRequest = new CurrencyActionRequestBuilder()
                .WithKey(string.Empty)
                .Build();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<RpcException>(() =>
                _sut.HandleAsync(invalidRequest));
            Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
        }

        [Fact]
        public async Task HandleAsync_CurrencyValidationFails_ThrowsValidationException()
        {
            // Arrange
            var request = new CurrencyActionRequestBuilder()
                .WithKey(Guid.NewGuid().ToString())
                .WithCurrency(new CurrencyBuilder().WithName(string.Empty).Build())
                .Build();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<RpcException>(() =>
                _sut.HandleAsync(request));
            Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
        }

        [Fact]
        public async Task HandleAsync_ManagerReturnsNotFound_ThrowsRpcException()
        {
            // Arrange
            var key = Guid.NewGuid();
            var request = new CurrencyActionRequestBuilder()
                .WithKey(key.ToString())
                .Build();

            _mocker.GetMock<IUpdateManager>()
                .Setup(m => m.UpdateAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CurrencyEntity>(),
                    It.IsAny<IEnumerable<DenominationEntity>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(ManagerResponse.NotFound());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<RpcException>(() =>
                _sut.HandleAsync(request));
            Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
        }

        [Fact]
        public async Task HandleAsync_ManagerThrowsException_ThrowsRpcException()
        {
            // Arrange
            var key = Guid.NewGuid();
            var request = new CurrencyActionRequestBuilder()
                .WithKey(key.ToString())
                .Build();

            _mocker.GetMock<IUpdateManager>()
                .Setup(m => m.UpdateAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CurrencyEntity>(),
                    It.IsAny<IEnumerable<DenominationEntity>>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<RpcException>(() =>
                _sut.HandleAsync(request));
            Assert.Equal(StatusCode.Internal, exception.Status.StatusCode);
        }

        [Fact]
        public async Task HandleAsync_MapperConfigurationIsValid()
        {
            // Arrange
            var key = Guid.NewGuid();
            var request = new CurrencyActionRequestBuilder()
                .WithKey(key.ToString())
                .WithCurrency(new CurrencyBuilder()
                    .WithDenominations(new List<Denomination> { new DenominationBuilder().Build() })
                    .Build())
                .Build();

            // Act & Assert
            await _sut.HandleAsync(request); // Should not throw mapping exceptions
        }
    }
}

