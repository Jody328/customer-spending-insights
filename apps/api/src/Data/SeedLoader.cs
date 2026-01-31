using System.Text.Json;
using CustomerSpending.Api.Common;
using CustomerSpending.Api.Models;
using CustomerSpending.Api.Models.Responses;

namespace CustomerSpending.Api.Data;

public sealed class SeedLoader
{
    private readonly InMemoryDataStore _store;
    private readonly IWebHostEnvironment _env;

    public SeedLoader(InMemoryDataStore store, IWebHostEnvironment env)
    {
        _store = store;
        _env = env;
    }

    public void LoadOrThrow()
    {
        if (_store.IsLoaded)
            return;

        var seedDir = Path.Combine(_env.ContentRootPath, "seed");

        // Fail fast with a helpful message if the seed folder is missing
        if (!Directory.Exists(seedDir))
            throw new InvalidOperationException($"Seed directory not found: {seedDir}");

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        // 1) Profile
        var profilePath = Path.Combine(seedDir, "profile.json");
        var profile = ReadJsonOrThrow<CustomerProfile>(profilePath, jsonOptions);
        _store.ProfilesByCustomerId[profile.CustomerId] = profile;

        // 2) Filters
        var filtersPath = Path.Combine(seedDir, "filters.json");
        _store.Filters = ReadJsonOrThrow<FiltersResponse>(filtersPath, jsonOptions);

        // 3) Goals
        var goalsPath = Path.Combine(seedDir, "goals.json");
        _store.Goals = ReadJsonOrThrow<SpendingGoalsResponse>(goalsPath, jsonOptions);

        // 4) Trends
        var trendsPath = Path.Combine(seedDir, "trends.json");
        _store.Trends = ReadJsonOrThrow<SpendingTrendsResponse>(trendsPath, jsonOptions);

        // 5) Transactions (bare array in the file)
        var transactionsPath = Path.Combine(seedDir, "transactions.json");
        var txns = ReadJsonOrThrow<List<Transaction>>(transactionsPath, jsonOptions);

        // basic sanity cleanup (sort descending by date)
        _store.Transactions.Clear();
        _store.Transactions.AddRange(txns.OrderByDescending(t => t.Date));

        _store.IsLoaded = true;
    }

    private static T ReadJsonOrThrow<T>(string path, JsonSerializerOptions options)
    {
        if (!File.Exists(path))
            throw new InvalidOperationException($"Seed file not found: {path}");

        var json = File.ReadAllText(path);

        if (string.IsNullOrWhiteSpace(json))
            throw new InvalidOperationException($"Seed file is empty: {path}");

        try
        {
            var value = JsonSerializer.Deserialize<T>(json, options);
            if (value is null)
                throw new InvalidOperationException($"Seed file could not be deserialized to {typeof(T).Name}: {path}");

            return value;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Invalid JSON in seed file: {path}. {ex.Message}", ex);
        }
    }
}
