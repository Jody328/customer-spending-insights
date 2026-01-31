using CustomerSpending.Api.Models;
using CustomerSpending.Api.Models.Responses;

namespace CustomerSpending.Api.Repositories;

public interface ICustomerRepository
{
    Task<CustomerProfile?> GetProfileAsync(string customerId, CancellationToken ct = default);

    Task<FiltersResponse?> GetFiltersAsync(string customerId, CancellationToken ct = default);

    Task<SpendingGoalsResponse?> GetGoalsAsync(string customerId, CancellationToken ct = default);

    Task<SpendingTrendsResponse?> GetTrendsAsync(string customerId, int months, CancellationToken ct = default);
}
