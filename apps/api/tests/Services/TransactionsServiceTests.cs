using CustomerSpending.Api.Models;
using CustomerSpending.Api.Models.Queries;
using CustomerSpending.Api.Repositories;
using CustomerSpending.Api.Services;
using Xunit;

namespace CustomerSpending.Api.Tests.Services;

public class TransactionsServiceTests
{
    private sealed class FakeClock : Common.IClock
    {
        public FakeClock(DateTime utcNow) => UtcNow = utcNow;
        public DateTime UtcNow { get; }
    }

    [Fact]
    public async Task Applies_Period_Filter_30d_By_Default_When_No_Dates_Provided()
    {
        // Create 40 transactions: 35 within last 35 days and 5 older
        var now = DateTimeOffset.Parse("2024-09-16T00:00:00Z");
        var txns = new List<Transaction>();

        for (int i = 0; i < 40; i++)
        {
            txns.Add(new Transaction(
                Id: $"t{i}",
                Date: now.AddDays(-i), // t0 = today, t39 = 39 days ago
                Merchant: "M",
                Category: "Groceries",
                Amount: 10,
                Description: "x",
                PaymentMethod: "Card",
                Icon: "shopping-cart",
                CategoryColor: "#FF6B6B"
            ));
        }

        var repo = new FakeTransactionRepo(txns);
        var clock = new FakeClock(new DateTime(2024, 09, 16, 12, 0, 0, DateTimeKind.Utc));
        var sut = new TransactionsService(repo, new DateRangeService(), clock);


        // No period, no custom dates => defaults to 30d (i.e., last 30 days inclusive: 0..29 days ago)
        var result = await sut.GetTransactionsAsync("12345",
            new TransactionsQuery(100, 0, null, null, null, null, "date_desc"));

        // Expect 30 transactions (days 0..29)
        Assert.Equal(30, result.Transactions.Count);
    }

    [Fact]
    public async Task CustomRange_Overrides_Period()
    {
        var txns = new List<Transaction>
    {
        new("old", DateTimeOffset.Parse("2024-08-01T00:00:00Z"), "M", "Groceries", 10, "x", "Card", "shopping-cart", "#FF6B6B"),
        new("inrange", DateTimeOffset.Parse("2024-09-05T00:00:00Z"), "M", "Groceries", 10, "x", "Card", "shopping-cart", "#FF6B6B")
    };

        var repo = new FakeTransactionRepo(txns);
        var clock = new FakeClock(new DateTime(2024, 09, 16, 12, 0, 0, DateTimeKind.Utc));
        var sut = new TransactionsService(repo, new DateRangeService(), clock);

        // period=7d would likely exclude 2024-09-05 if "now" is later,
        // but custom range explicitly includes it.
        var result = await sut.GetTransactionsAsync("12345",
            new TransactionsQuery(
                Limit: 100,
                Offset: 0,
                Category: null,
                Period: "7d",
                StartDate: "2024-09-01",
                EndDate: "2024-09-10",
                SortBy: "date_desc"
            ));

        Assert.Single(result.Transactions);
        Assert.Equal("inrange", result.Transactions[0].Id);
    }



    [Fact]
    public async Task Applies_Pagination_And_HasMore()
    {
        var repo = new FakeTransactionRepo(GenerateTransactions(50));
        var dateRanges = new DateRangeService();
        var clock = new FakeClock(new DateTime(2024, 09, 16, 12, 0, 0, DateTimeKind.Utc));
        var sut = new TransactionsService(repo, new DateRangeService(), clock);


        var result = await sut.GetTransactionsAsync("12345", new TransactionsQuery(
            Limit: 20,
            Offset: 0,
            Category: null,
            Period: "1y",
            StartDate: null,
            EndDate: null,
            SortBy: "date_desc"
        ));

        Assert.Equal(50, result.Pagination.Total);
        Assert.Equal(20, result.Transactions.Count);
        Assert.True(result.Pagination.HasMore);
    }

    [Fact]
    public async Task Applies_Category_Filter()
    {
        var txns = new List<Transaction>
        {
            new("1", DateTimeOffset.Parse("2024-09-01T00:00:00Z"), "A", "Groceries", 10, "d", "Card", "shopping-cart", "#fff"),
            new("2", DateTimeOffset.Parse("2024-09-02T00:00:00Z"), "B", "Entertainment", 20, "d", "Card", "film", "#fff")
        };

        var repo = new FakeTransactionRepo(txns);
        var clock = new FakeClock(new DateTime(2024, 09, 16, 12, 0, 0, DateTimeKind.Utc));
        var sut = new TransactionsService(repo, new DateRangeService(), clock);


        var result = await sut.GetTransactionsAsync("12345", new TransactionsQuery(
            Limit: 20,
            Offset: 0,
            Category: "Groceries",
            Period: null,
            StartDate: null,
            EndDate: null,
            SortBy: "date_desc"
        ));

        Assert.Single(result.Transactions);
        Assert.Equal("Groceries", result.Transactions[0].Category);
    }

    [Fact]
    public async Task Applies_Sort_AmountDesc()
    {
        var txns = new List<Transaction>
        {
            new("1", DateTimeOffset.Parse("2024-09-01T00:00:00Z"), "A", "Groceries", 10, "d", "Card", "shopping-cart", "#fff"),
            new("2", DateTimeOffset.Parse("2024-09-02T00:00:00Z"), "B", "Groceries", 50, "d", "Card", "shopping-cart", "#fff"),
            new("3", DateTimeOffset.Parse("2024-09-03T00:00:00Z"), "C", "Groceries", 20, "d", "Card", "shopping-cart", "#fff")
        };

        var repo = new FakeTransactionRepo(txns);
        var clock = new FakeClock(new DateTime(2024, 09, 16, 12, 0, 0, DateTimeKind.Utc));
        var sut = new TransactionsService(repo, new DateRangeService(), clock);


        var result = await sut.GetTransactionsAsync("12345", new TransactionsQuery(
            Limit: 100,
            Offset: 0,
            Category: null,
            Period: null,
            StartDate: null,
            EndDate: null,
            SortBy: "amount_desc"
        ));

        Assert.Equal("2", result.Transactions[0].Id);
    }

    private static List<Transaction> GenerateTransactions(int count)
    {
        var list = new List<Transaction>();
        var baseDate = DateTimeOffset.Parse("2024-09-16T00:00:00Z");

        for (var i = 0; i < count; i++)
        {
            list.Add(new Transaction(
                Id: $"txn_{i}",
                Date: baseDate.AddDays(-i),
                Merchant: "Test",
                Category: "Groceries",
                Amount: i + 1,
                Description: "x",
                PaymentMethod: "Card",
                Icon: "shopping-cart",
                CategoryColor: "#FF6B6B"
            ));
        }

        return list;
    }

    private sealed class FakeTransactionRepo : ITransactionRepository
    {
        private readonly IReadOnlyList<Transaction> _txns;
        public FakeTransactionRepo(IReadOnlyList<Transaction> txns) => _txns = txns;
        public Task<IReadOnlyList<Transaction>> GetAllAsync(string customerId, CancellationToken ct = default) => Task.FromResult(_txns);
    }
}
