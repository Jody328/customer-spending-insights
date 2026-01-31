using CustomerSpending.Api.Common;
using CustomerSpending.Api.Models;
using CustomerSpending.Api.Models.Queries;
using CustomerSpending.Api.Models.Responses;
using CustomerSpending.Api.Repositories;
using CustomerSpending.Api.Services;
using Xunit;

namespace CustomerSpending.Api.Tests.Services;

public class SpendingServiceTests
{
    [Fact]
    public async Task Summary_Computes_Totals_And_Counts()
    {
        var now = new DateTime(2024, 09, 16, 12, 0, 0, DateTimeKind.Utc);
        var clock = new FakeClock(now);

        var txns = new List<Transaction>
        {
            new("1", DateTimeOffset.Parse("2024-09-16T10:00:00Z"), "A", "Groceries", 100, "x", "Card", "shopping-cart", "#FF6B6B"),
            new("2", DateTimeOffset.Parse("2024-09-15T10:00:00Z"), "B", "Groceries", 50, "x", "Card", "shopping-cart", "#FF6B6B"),
            new("3", DateTimeOffset.Parse("2024-09-10T10:00:00Z"), "C", "Dining", 25, "x", "Card", "utensils", "#F7DC6F"),
        };

        var spending = CreateSut(txns, clock);

        var result = await spending.GetSummaryAsync("12345", "30d");

        Assert.Equal(175m, result.TotalSpent);
        Assert.Equal(3, result.TransactionCount);
        Assert.Equal("Groceries", result.TopCategory);
    }

    [Fact]
    public async Task Categories_Aggregates_And_Enriches_With_Filter_Metadata()
    {
        var now = new DateTime(2024, 09, 16, 12, 0, 0, DateTimeKind.Utc);
        var clock = new FakeClock(now);

        var txns = new List<Transaction>
        {
            new("1", DateTimeOffset.Parse("2024-09-16T10:00:00Z"), "A", "Groceries", 100, "x", "Card", "shopping-cart", "#FF6B6B"),
            new("2", DateTimeOffset.Parse("2024-09-15T10:00:00Z"), "B", "Entertainment", 50, "x", "Card", "film", "#4ECDC4"),
            new("3", DateTimeOffset.Parse("2024-09-15T11:00:00Z"), "C", "Entertainment", 25, "x", "Card", "film", "#4ECDC4"),
        };

        var spending = CreateSut(txns, clock);

        var result = await spending.GetCategoriesAsync("12345", new SpendingCategoriesQuery("30d", null, null));

        Assert.Equal(175m, result.TotalAmount);
        Assert.Equal(2, result.Categories.Count);

        var entertainment = result.Categories.First(c => c.Name == "Entertainment");
        Assert.Equal(75m, entertainment.Amount);
        Assert.Equal("film", entertainment.Icon);
        Assert.Equal("#4ECDC4", entertainment.Color);
        Assert.Equal(2, entertainment.TransactionCount);
    }

    private static ISpendingService CreateSut(List<Transaction> txns, IClock clock)
    {
        var txRepo = new FakeTxRepo(txns);
        var customerRepo = new FakeCustomerRepo();
        var dateRanges = new DateRangeService();

        return new SpendingService(txRepo, customerRepo, dateRanges, clock);
    }

    private sealed class FakeClock : IClock
    {
        public FakeClock(DateTime utcNow) => UtcNow = utcNow;
        public DateTime UtcNow { get; }
    }

    private sealed class FakeTxRepo : ITransactionRepository
    {
        private readonly IReadOnlyList<Transaction> _txns;
        public FakeTxRepo(IReadOnlyList<Transaction> txns) => _txns = txns;
        public Task<IReadOnlyList<Transaction>> GetAllAsync(string customerId, CancellationToken ct = default) => Task.FromResult(_txns);
    }

    private sealed class FakeCustomerRepo : ICustomerRepository
    {
        public Task<CustomerProfile?> GetProfileAsync(string customerId, CancellationToken ct = default) => Task.FromResult<CustomerProfile?>(null);

        public Task<FiltersResponse?> GetFiltersAsync(string customerId, CancellationToken ct = default)
        {
            var filters = new FiltersResponse(
                Categories: new List<Models.Responses.CategoryFilter>
                {
                    new("Groceries", "#FF6B6B", "shopping-cart"),
                    new("Entertainment", "#4ECDC4", "film")
                },
                DateRangePresets: new List<Models.Responses.DateRangePreset>()
            );

            return Task.FromResult<FiltersResponse?>(filters);
        }

        public Task<SpendingGoalsResponse?> GetGoalsAsync(string customerId, CancellationToken ct = default) => Task.FromResult<SpendingGoalsResponse?>(null);

        public Task<SpendingTrendsResponse?> GetTrendsAsync(string customerId, int months, CancellationToken ct = default) => Task.FromResult<SpendingTrendsResponse?>(null);
    }
}
