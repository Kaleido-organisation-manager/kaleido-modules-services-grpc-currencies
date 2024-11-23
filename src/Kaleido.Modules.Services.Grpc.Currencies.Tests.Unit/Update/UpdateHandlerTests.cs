using Xunit;
using Moq;
using Moq.AutoMock;
using Grpc.Core;
using Kaleido.Common.Services.Grpc.Models.Validations;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.Update;
using Kaleido.Common.Services.Grpc.Exceptions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
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

        public UpdateHandlerTests()
        {
            _mocker = new AutoMocker();

            var key = Guid.NewGuid();
            var validRevision = new EntityLifeCycleResult<CurrencyEntity, BaseRevisionEntity>
            {
                Entity = new CurrencyEntityBuilder().Build(),
                Revision = new CurrencyRevisionBuilder().WithKey(key).WithRevision(1).Build()
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
                .Setup(m => m.UpdateAsync(It.IsAny<Guid>(), It.IsAny<CurrencyEntity>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid key, CurrencyEntity entity, CancellationToken cancellationToken) =>
                {
                    validRevision.Revision.Key = key;
                    validRevision.Entity = entity;
                    return ManagerResponse.Success(validRevision);
                });

            _sut = _mocker.CreateInstance<UpdateHandler>();
        }

        [Fact]
        public async Task HandleAsync_ValidRequest_ReturnsUpdateCurrencyResponse()
        {
            // Arrange
            var key = Guid.NewGuid();
            var request = new CurrencyActionRequestBuilder().WithKey(key.ToString()).Build();

            // Act
            var result = await _sut.HandleAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CurrencyResponse>(result);
            Assert.NotNull(result.Currency);
        }

        [Fact]
        public async Task HandleAsync_ValidRequest_CallsManager()
        {
            // Arrange
            var key = Guid.NewGuid();
            var request = new CurrencyActionRequestBuilder().WithKey(key.ToString()).Build();

            // Act
            await _sut.HandleAsync(request);

            // Assert
            _mocker.GetMock<IUpdateManager>()
                .Verify(m => m.UpdateAsync(It.IsAny<Guid>(), It.IsAny<CurrencyEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_ValidationFails_ThrowsValidationException()
        {
            // Arrange
            var invalidRequest = new CurrencyActionRequestBuilder().WithKey(string.Empty).Build();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<RpcException>(() => _sut.HandleAsync(invalidRequest));
            Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
        }

        [Fact]
        public async Task HandleAsync_ManagerReturnsNotFound_ThrowsRpcException()
        {
            // Arrange
            var key = Guid.NewGuid();
            var request = new CurrencyActionRequestBuilder().WithKey(key.ToString()).Build();

            _mocker.GetMock<IUpdateManager>()
                .Setup(m => m.UpdateAsync(It.IsAny<Guid>(), It.IsAny<CurrencyEntity>(), It.IsAny<CancellationToken>()))
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
            var request = new CurrencyActionRequestBuilder().WithKey(key.ToString()).Build();

            _mocker.GetMock<IUpdateManager>()
                .Setup(m => m.UpdateAsync(It.IsAny<Guid>(), It.IsAny<CurrencyEntity>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<RpcException>(() => _sut.HandleAsync(request));
            Assert.Equal(StatusCode.Internal, exception.Status.StatusCode);
        }
    }
}

