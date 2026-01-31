using CustomerSpending.Api.Data;
using CustomerSpending.Api.Repositories;
using CustomerSpending.Api.Endpoints;
using CustomerSpending.Api.Services;
using CustomerSpending.Api.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Abstracts
builder.Services.AddSingleton<IClock, SystemClock>();

// Services
builder.Services.AddScoped<IDateRangeService, DateRangeService>();
builder.Services.AddScoped<ITransactionsService, TransactionsService>();
builder.Services.AddScoped<ISpendingService, SpendingService>();


// Repositories
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

// Seed store + loader
builder.Services.AddSingleton<InMemoryDataStore>();
builder.Services.AddSingleton<SeedLoader>();

var app = builder.Build();

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Load seed at startup (fail fast)
app.Services.GetRequiredService<SeedLoader>().LoadOrThrow();

app.MapGet("/health", () => Results.Ok("OK"));

// Temporary debug endpoint
app.MapGet("/debug/seed-status", (InMemoryDataStore store) => Results.Ok(new
{
    store.IsLoaded,
    Customers = store.ProfilesByCustomerId.Keys,
    Transactions = store.Transactions.Count
}));

CustomersEndpoints.Map(app);

if (app.Environment.IsDevelopment())
{
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

app.Run();
