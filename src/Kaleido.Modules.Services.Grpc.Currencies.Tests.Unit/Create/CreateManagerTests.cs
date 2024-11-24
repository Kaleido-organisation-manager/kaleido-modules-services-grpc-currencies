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

        _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity>>()
            .Setup(r => r.CreateAsync(currencyEntity, It.IsAny<CurrencyRevisionEntity?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CurrencyEntity c, CurrencyRevisionEntity? r, CancellationToken ct) =>
            new EntityLifeCycleResult<CurrencyEntity, CurrencyRevisionEntity>
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
        var denominations = new List<DenominationEntity>()
        {
            new DenominationEntityBuilder().Build()
        };

        // Act
        await _sut.CreateAsync(currencyEntity, denominations, CancellationToken.None);

        // Assert
        _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity>>()
            .Verify(r => r.CreateAsync(currencyEntity, It.IsAny<CurrencyRevisionEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldCallRepositoryCreateAsyncForDenominations()
    {
        // Arrange
        var currencyEntity = new CurrencyEntityBuilder().Build();
        var denominations = new List<DenominationEntity>()
        {
            new DenominationEntityBuilder().WithValue(100).Build(),
            new DenominationEntityBuilder().WithValue(50).Build(),
            new DenominationEntityBuilder().WithValue(20).Build(),
            new DenominationEntityBuilder().WithValue(10).Build(),
            new DenominationEntityBuilder().WithValue(5).Build(),
            new DenominationEntityBuilder().WithValue(1).Build(),
            new DenominationEntityBuilder().WithValue(0.5M).Build(),
            new DenominationEntityBuilder().WithValue(0.2M).Build(),
            new DenominationEntityBuilder().WithValue(0.1M).Build(),
            new DenominationEntityBuilder().WithValue(0.05M).Build(),
            new DenominationEntityBuilder().WithValue(0.01M).Build()
        };

        // Act
        await _sut.CreateAsync(currencyEntity, denominations, CancellationToken.None);

        // Assert
        _mocker.GetMock<IEntityLifecycleHandler<DenominationEntity, DenominationRevisionEntity>>()
            .Verify(r => r.CreateAsync(It.IsAny<DenominationEntity>(), It.IsAny<DenominationRevisionEntity>(), It.IsAny<CancellationToken>()), Times.Exactly(denominations.Count));
    }

    [Fact]
    public async Task CreateAsync_ShouldPassCancellationTokenToRepository()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var currencyEntity = new CurrencyEntityBuilder().Build();
        var denominations = new List<DenominationEntity>()
        {
            new DenominationEntityBuilder().Build()
        };
        // Act
        await _sut.CreateAsync(currencyEntity, denominations, cancellationToken);

        // Assert
        _mocker.GetMock<IEntityLifecycleHandler<CurrencyEntity, CurrencyRevisionEntity>>()
            .Verify(r => r.CreateAsync(currencyEntity, It.IsAny<CurrencyRevisionEntity>(), cancellationToken), Times.Once);
    }
}
