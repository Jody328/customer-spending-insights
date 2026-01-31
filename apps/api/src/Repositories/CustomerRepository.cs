using CustomerSpending.Api.Data;
using CustomerSpending.Api.Models;
using CustomerSpending.Api.Models.Responses;

namespace CustomerSpending.Api.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly InMemoryDataStore _store;

    public CustomerRepository(InMemoryDataStore store)
    {
        _store = store;
    }

    public Task<CustomerProfile?> GetProfileAsync(string customerId, CancellationToken ct = default)
    {
        _store.ProfilesByCustomerId.TryGetValue(customerId, out var profile);
        return Task.FromResult(profile);
    }

    public Task<FiltersResponse?> GetFiltersAsync(string customerId, CancellationToken ct = default)
    {
        return Task.FromResult(_store.Filters);
    }

    public Task<SpendingGoalsResponse?> GetGoalsAsync(string customerId, CancellationToken ct = default)
    {
        return Task.FromResult(_store.Goals);
    }

    public Task<SpendingTrendsResponse?> GetTrendsAsync(string customerId, int months, CancellationToken ct = default)
    {
        var trends = _store.Trends;
        if (trends is null) return Task.FromResult<SpendingTrendsResponse?>(null);

        // months is validated higher up; here we just slice the last N months available
        var sliced = trends.Trends
            .OrderBy(t => t.Month)
            .TakeLast(months)
            .ToList();

        return Task.FromResult<SpendingTrendsResponse?>(new SpendingTrendsResponse(sliced));
    }
}
