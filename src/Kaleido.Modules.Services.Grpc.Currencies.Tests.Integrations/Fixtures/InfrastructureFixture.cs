using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using Grpc.Net.Client;
using Kaleido.Grpc.Currencies;
using Kaleido.Modules.Services.Grpc.Currencies.Tests.Integrations.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using static Kaleido.Grpc.Currencies.GrpcCurrencies;

namespace Kaleido.Modules.Services.Grpc.Currencies.Tests.Integrations.Fixtures;

public class InfrastructureFixture : IDisposable
{
    private const int TIMEOUT_WAIT_MINUTES = 1;
    private const string DB_NAME = "currencies";
    private const string DB_USER = "postgres";
    private const string DB_PASSWORD = "postgres";

    private string _migrationImageName = "kaleido-modules-services-grpc-currencies-migrations:latest";
    private string _grpcImageName = "kaleido-modules-services-grpc-currencies:latest";
    private readonly bool _isLocalDevelopment;
    private IFutureDockerImage? _grpcImage;
    private IFutureDockerImage? _migrationImage;
    private IContainer _migrationContainer = null!;
    private PostgreSqlContainer _postgres { get; }
    private GrpcChannel _channel { get; set; } = null!;

    public GrpcCurrenciesClient Client { get; private set; } = null!;
    public IContainer GrpcContainer { get; private set; } = null!;
    public string ConnectionString { get; private set; } = null!;

    public InfrastructureFixture()
    {
        // Read from environment variables or appsettings.json file
        var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.Development.json", optional: true)
        .AddEnvironmentVariables()
        .Build();

        _isLocalDevelopment = configuration.GetValue<bool?>("CI") == null;

        if (!_isLocalDevelopment)
        {
            _grpcImageName = configuration.GetValue<string>("CURRENCIES_IMAGE_NAME") ?? _grpcImageName;
            _migrationImageName = configuration.GetValue<string>("MIGRATIONS_IMAGE_NAME") ?? _migrationImageName;
        }

        _postgres = new PostgreSqlBuilder()
            .WithDatabase(DB_NAME)
            .WithUsername(DB_USER)
            .WithPassword(DB_PASSWORD)
            .WithLogger(new LoggerFactory().CreateLogger<PostgreSqlContainer>())
            .WithPortBinding(5432, true)
            .WithExposedPort(5432)
            .WithNetworkAliases("postgres")
            .WithHostname("postgres")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilMessageIsLogged("database system is ready to accept connections"))
            .Build();

        if (_isLocalDevelopment)
        {
            var nugetUser = configuration.GetValue<string>("NUGET_USER");
            var nugetToken = configuration.GetValue<string>("NUGET_TOKEN");

            _migrationImage = new ImageFromDockerfileBuilder()
                .WithDockerfileDirectory(Path.Join(CommonDirectoryPath.GetSolutionDirectory().DirectoryPath, "../"))
                .WithDockerfile("dockerfiles/Grpc.Currencies.Migrations/Dockerfile.local")
                .WithName(_migrationImageName)
                .WithLogger(new LoggerFactory().CreateLogger<ImageFromDockerfileBuilder>())
                .WithBuildArgument("NUGET_USER", nugetUser)
                .WithBuildArgument("NUGET_TOKEN", nugetToken)
                .Build();

            _grpcImage = new ImageFromDockerfileBuilder()
                .WithDockerfileDirectory(Path.Join(CommonDirectoryPath.GetSolutionDirectory().DirectoryPath, "../"))
                .WithDockerfile("dockerfiles/Grpc.Currencies/Dockerfile.local")
                .WithName(_grpcImageName)
                .WithLogger(new LoggerFactory().CreateLogger<ImageFromDockerfileBuilder>())
                .WithBuildArgument("NUGET_USER", nugetUser)
                .WithBuildArgument("NUGET_TOKEN", nugetToken)
                .Build();
        }

        InitializeAsync().Wait();
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync().WaitAsync(TimeSpan.FromMinutes(TIMEOUT_WAIT_MINUTES));
        await _postgres.WaitForPort().WaitAsync(TimeSpan.FromMinutes(TIMEOUT_WAIT_MINUTES));

        if (_migrationImage != null)
        {
            await _migrationImage.CreateAsync().WaitAsync(TimeSpan.FromMinutes(TIMEOUT_WAIT_MINUTES));
        }

        if (_grpcImage != null)
        {
            await _grpcImage.CreateAsync().WaitAsync(TimeSpan.FromMinutes(TIMEOUT_WAIT_MINUTES));
        }


        string host = "host.testcontainers.internal";
        var postgresPort = _postgres.GetMappedPublicPort(5432);
        ConnectionString = $"Server={host};Port={postgresPort};Database={DB_NAME};Username={DB_USER};Password={DB_PASSWORD}";
        await TestcontainersSettings.ExposeHostPortsAsync(postgresPort)
            .ConfigureAwait(false);

        _migrationContainer = new ContainerBuilder()
            .WithImage(_migrationImageName)
            .WithEnvironment("ConnectionStrings:Currencies", ConnectionString)
            .DependsOn(_postgres)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Migration completed successfully."))
            .WithLogger(new LoggerFactory().CreateLogger<IContainer>())
            .Build();

        await _migrationContainer.StartAsync().WaitAsync(TimeSpan.FromMinutes(TIMEOUT_WAIT_MINUTES));

        GrpcContainer = new ContainerBuilder()
            .WithImage(_grpcImageName)
            .WithPortBinding(8080, true)
            .WithExposedPort(8080)
            .DependsOn(_migrationContainer)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8080))
            .WithEnvironment("ConnectionStrings:Currencies", ConnectionString)
            .WithLogger(new LoggerFactory().CreateLogger<IContainer>())
            .Build();

        await GrpcContainer.StartAsync().WaitAsync(TimeSpan.FromMinutes(TIMEOUT_WAIT_MINUTES));


        var grpcPort = GrpcContainer.GetMappedPublicPort(8080);
        await TestcontainersSettings.ExposeHostPortsAsync(grpcPort);
        var grpcUri = new UriBuilder("http", GrpcContainer.Hostname, grpcPort);
        _channel = GrpcChannel.ForAddress(grpcUri.Uri);

        Client = new GrpcCurrenciesClient(_channel);
    }

    public async Task DisposeAsync()
    {
        await _migrationContainer.DisposeAsync();
        await _postgres.DisposeAsync();
        _channel.Dispose();
        await GrpcContainer.DisposeAsync();
    }

    public void Dispose()
    {
        DisposeAsync().Wait();
    }

    public async Task ClearDatabase()
    {
        // TODO: Implement
        var currencies = await Client.GetAllCurrenciesAsync(new EmptyRequest());
        foreach (var currency in currencies.Currencies)
        {
            if (currency.Revision.Action != "Deleted")
                await Client.DeleteCurrencyAsync(new CurrencyRequest { Key = currency.Key });
        }
    }
}


[CollectionDefinition("Infrastructure collection")]
public class InfrastructureCollection : ICollectionFixture<InfrastructureFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}