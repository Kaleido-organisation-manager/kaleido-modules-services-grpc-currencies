using Moq;
using Moq.AutoMock;
using Kaleido.Modules.Services.Grpc.Currencies.Create;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;
using AutoMapper;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Mappers;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Common.Builders;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Unit.Create;

public class CreateManagerTests
{
    private readonly AutoMocker _mocker;
    private readonly CreateManager _sut;

    public CreateManagerTests()
    {
        _mocker = new AutoMocker();
        _sut = _mocker.CreateInstance<CreateManager>();

        var currencyEntity = new CurrencyEntityBuilder().Build();

        _mocker.Use(() =>
        {
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CurrencyMappingProfile>();
            });
            return mapper.CreateMapper();
        });

        _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity>>()
            .Setup(r => r.CreateAsync(currencyEntity, It.IsAny<BaseRevisionEntity?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CurrencyEntity c, BaseRevisionEntity? r, CancellationToken ct) =>
            new EntityLifeCycleResult<CurrencyEntity, BaseRevisionEntity>
            {
                Entity = c,
                Revision = r ?? new CurrencyRevisionBuilder().Build()
            });
    }

    [Fact]
    public async Task CreateAsync_ShouldCallRepositoryCreateAsync()
    {
        // Arrange
        var currencyEntity = new CurrencyEntityBuilder().Build();

        // Act
        await _sut.CreateAsync(currencyEntity);

        // Assert
        _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity>>()
            .Verify(r => r.CreateAsync(currencyEntity, It.IsAny<BaseRevisionEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact]
    public async Task CreateAsync_ShouldPassCancellationTokenToRepository()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var currencyEntity = new CurrencyEntityBuilder().Build();
        // Act
        await _sut.CreateAsync(currencyEntity, cancellationToken);

        // Assert
        _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, BaseRevisionEntity>>()
            .Verify(r => r.CreateAsync(currencyEntity, It.IsAny<BaseRevisionEntity>(), cancellationToken), Times.Once);
    }
}
