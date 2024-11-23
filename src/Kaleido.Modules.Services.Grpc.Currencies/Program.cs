using Kaleido.Common.Services.Grpc.Configuration.Extensions;
using Kaleido.Common.Services.Grpc.Handlers.Extensions;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Common.Services.Grpc.Repositories.Extensions;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Configuration;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Mappers;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Models;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Services;
using Kaleido.Modules.Services.Grpc.Currencies.Common.Validators;
using Kaleido.Modules.Services.Grpc.Currencies.Create;
using Kaleido.Modules.Services.Grpc.Currencies.Delete;
using Kaleido.Modules.Services.Grpc.Currencies.Get;
using Kaleido.Modules.Services.Grpc.Currencies.GetAll;
using Kaleido.Modules.Services.Grpc.Currencies.GetAllFiltered;
using Kaleido.Modules.Services.Grpc.Currencies.GetAllRevisions;
using Kaleido.Modules.Services.Grpc.Currencies.GetRevision;
using Kaleido.Modules.Services.Grpc.Currencies.Update;

var builder = WebApplication.CreateBuilder(args);

// Common
builder.Services.AddAutoMapper(typeof(CurrencyMappingProfile));
builder.Services.AddScoped<CurrencyValidator>();
builder.Services.AddScoped<KeyValidator>();
builder.Services.AddScoped<NameValidator>();

var Configuration = builder.Configuration;
var currenciesConnectionString = Configuration.GetConnectionString("currencies");
if (string.IsNullOrEmpty(currenciesConnectionString))
{
    throw new ArgumentNullException(nameof(currenciesConnectionString), "No connection string found to connect to the currencies database");
}

builder.Services.AddKaleidoEntityDbContext<CurrencyEntity, CurrencyEntityDbContext>(currenciesConnectionString);
builder.Services.AddKaleidoRevisionDbContext<BaseRevisionEntity, CurrencyEntityRevisionDbContext>(currenciesConnectionString);
builder.Services.AddEntityRepository<CurrencyEntity, CurrencyEntityDbContext>();
builder.Services.AddRevisionRepository<CurrencyEntityRevisionDbContext>();
builder.Services.AddLifeCycleHandler<CurrencyEntity>();


// Create
builder.Services.AddScoped<ICreateHandler, CreateHandler>();
builder.Services.AddScoped<ICreateManager, CreateManager>();

// Delete
builder.Services.AddScoped<IDeleteHandler, DeleteHandler>();
builder.Services.AddScoped<IDeleteManager, DeleteManager>();

// Get
builder.Services.AddScoped<IGetHandler, GetHandler>();
builder.Services.AddScoped<IGetManager, GetManager>();

// GetAll
builder.Services.AddScoped<IGetAllHandler, GetAllHandler>();
builder.Services.AddScoped<IGetAllManager, GetAllManager>();

// GetAllByName
builder.Services.AddScoped<IGetAllFilteredHandler, GetAllFilteredHandler>();
builder.Services.AddScoped<IGetAllFilteredManager, GetAllFilteredManager>();

// GetAllRevisions
builder.Services.AddScoped<IGetAllRevisionsHandler, GetAllRevisionsHandler>();
builder.Services.AddScoped<IGetAllRevisionsManager, GetAllRevisionsManager>();

// GetRevision
builder.Services.AddScoped<IGetRevisionHandler, GetRevisionHandler>();
builder.Services.AddScoped<IGetRevisionManager, GetRevisionManager>();

// Update
builder.Services.AddScoped<IUpdateHandler, UpdateHandler>();
builder.Services.AddScoped<IUpdateManager, UpdateManager>();

// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<CurrencyService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
