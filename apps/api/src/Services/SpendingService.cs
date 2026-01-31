using CustomerSpending.Api.Common;
using CustomerSpending.Api.Models;
using CustomerSpending.Api.Models.Queries;
using CustomerSpending.Api.Models.Responses;
using CustomerSpending.Api.Repositories;

namespace CustomerSpending.Api.Services;

public sealed class SpendingService : ISpendingService
{
    private readonly ITransactionRepository _transactions;
    private readonly ICustomerRepository _customers;
    private readonly IDateRangeService _dateRanges;
    private readonly IClock _clock;

    public SpendingService(
        ITransactionRepository transactions,
        ICustomerRepository customers,
        IDateRangeService dateRanges,
        IClock clock)
    {
        _transactions = transactions;
        _customers = customers;
        _dateRanges = dateRanges;
        _clock = clock;
    }

    public async Task<SpendingSummaryResponse> GetSummaryAsync(string customerId, string? period, CancellationToken ct = default)
    {
        var all = await _transactions.GetAllAsync(customerId, ct);
        var referenceNow = _dateRanges.GetReferenceUtcNow(all, _clock.UtcNow);

        // Default period is 30d if not supplied (as per spec)
        var currentRange = _dateRanges.Resolve(period, startDate: null, endDate: null, utcNow: referenceNow);
        var currentTxns = FilterByRange(all, currentRange);
        var currentTotal = currentTxns.Sum(t => t.Amount);
        var currentCount = currentTxns.Count;
        var currentAvg = currentCount == 0 ? 0m : Decimal.Round(currentTotal / currentCount, 2);

        var topCategory = currentCount == 0
            ? string.Empty
            : currentTxns
                .GroupBy(t => t.Category, StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(g => g.Sum(x => x.Amount))
                .First().Key;

        
        var days = (currentRange.EndUtcInclusive.Date - currentRange.StartUtcInclusive.Date).Days + 1;
        var prevEndDate = currentRange.StartUtcInclusive.Date.AddDays(-1);
        var prevStartDate = prevEndDate.AddDays(-(days - 1));

        var prevRange = new Models.Queries.DateRange(
            StartUtcInclusive: DateTime.SpecifyKind(prevStartDate, DateTimeKind.Utc),
            EndUtcInclusive: DateTime.SpecifyKind(prevEndDate, DateTimeKind.Utc).AddDays(1).AddTicks(-1),
            StartDate: prevStartDate.ToString("yyyy-MM-dd"),
            EndDate: prevEndDate.ToString("yyyy-MM-dd")
        );

        var prevTxns = FilterByRange(all, prevRange);
        var prevTotal = prevTxns.Sum(t => t.Amount);
        var prevCount = prevTxns.Count;

        var spentChange = PercentageChange(prevTotal, currentTotal);
        var txnChange = PercentageChange(prevCount, currentCount);

        return new SpendingSummaryResponse(
            Period: string.IsNullOrWhiteSpace(period) ? "30d" : period.Trim(),
            TotalSpent: Decimal.Round(currentTotal, 2),
            TransactionCount: currentCount,
            AverageTransaction: currentAvg,
            TopCategory: topCategory,
            ComparedToPrevious: new ComparedToPrevious(
                SpentChange: Decimal.Round(spentChange, 2),
                TransactionChange: Decimal.Round(txnChange, 2)
            )
        );
    }

    public async Task<SpendingByCategoryResponse> GetCategoriesAsync(string customerId, SpendingCategoriesQuery query, CancellationToken ct = default)
    {
        var all = await _transactions.GetAllAsync(customerId, ct);
        var referenceNow = _dateRanges.GetReferenceUtcNow(all, _clock.UtcNow);

        // Period default 30d; custom start/end takes precedence (handled inside service)
        var range = _dateRanges.Resolve(
            period: query.Period,
            startDate: query.StartDate,
            endDate: query.EndDate,
            utcNow: referenceNow
        );

        var filters = await _customers.GetFiltersAsync(customerId, ct);
        if (filters is null)
            throw new ArgumentException("Filters not found for customer.");

        var metaByName = filters.Categories.ToDictionary(
            c => c.Name,
            c => c,
            StringComparer.OrdinalIgnoreCase);

        var txns = FilterByRange(all, range);

        var total = txns.Sum(t => t.Amount);

        var categories = txns
            .GroupBy(t => t.Category, StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var amount = g.Sum(x => x.Amount);
                var pct = total == 0 ? 0m : (amount / total) * 100m;

                metaByName.TryGetValue(g.Key, out var meta);

                return new CategorySpendDto(
                    Name: g.Key,
                    Amount: Decimal.Round(amount, 2),
                    Percentage: Decimal.Round(pct, 1),
                    TransactionCount: g.Count(),
                    Color: meta?.Color ?? "#999999",
                    Icon: meta?.Icon ?? "tag"
                );
            })
            .OrderByDescending(x => x.Amount)
            .ToList();

        return new SpendingByCategoryResponse(
            DateRange: new DateRangeDto(range.StartDate, range.EndDate),
            TotalAmount: Decimal.Round(total, 2),
            Categories: categories
        );
    }

    private static List<Transaction> FilterByRange(IReadOnlyList<Transaction> all, Models.Queries.DateRange range)
    {
        return all
            .Where(t => t.Date.UtcDateTime >= range.StartUtcInclusive && t.Date.UtcDateTime <= range.EndUtcInclusive)
            .ToList();
    }

    private static decimal PercentageChange(decimal previous, decimal current)
    {
        if (previous == 0m)
            return current == 0m ? 0m : 100m;

        return ((current - previous) / previous) * 100m;
    }

    private static decimal PercentageChange(int previous, int current)
    {
        if (previous == 0)
            return current == 0 ? 0m : 100m;

        return ((decimal)(current - previous) / previous) * 100m;
    }
}
