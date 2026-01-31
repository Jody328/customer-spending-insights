using CustomerSpending.Api.Models;
using CustomerSpending.Api.Models.Responses;

namespace CustomerSpending.Api.Data;

public sealed class InMemoryDataStore
{
    public Dictionary<string, CustomerProfile> ProfilesByCustomerId { get; } = new();

    public FiltersResponse? Filters { get; set; }
    public SpendingGoalsResponse? Goals { get; set; }
    public SpendingTrendsResponse? Trends { get; set; }

    public List<Transaction> Transactions { get; } = new();

    public bool IsLoaded { get; set; }
}
