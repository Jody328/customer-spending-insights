using CustomerSpending.Api.Common;
using CustomerSpending.Api.Models.Queries;
using CustomerSpending.Api.Models.Responses;
using CustomerSpending.Api.Repositories;

namespace CustomerSpending.Api.Services;

public sealed class TransactionsService : ITransactionsService
{
    private readonly ITransactionRepository _transactions;
    private readonly IDateRangeService _dateRanges;
    private readonly IClock _clock;

    public TransactionsService(ITransactionRepository transactions, IDateRangeService dateRanges, IClock clock)
    {
        _transactions = transactions;
        _dateRanges = dateRanges;
        _clock = clock;
    }

    public async Task<TransactionsResponse> GetTransactionsAsync(string customerId, TransactionsQuery query, CancellationToken ct = default)
    {
        var all = await _transactions.GetAllAsync(customerId, ct);
        var referenceNow = _dateRanges.GetReferenceUtcNow(all, _clock.UtcNow);

        var limit = query.Limit <= 0 ? 20 : query.Limit;
        if (limit > 100) throw new ArgumentException("limit must be between 1 and 100");

        var offset = query.Offset < 0 ? 0 : query.Offset;

        var sort = TransactionSortByParser.Parse(query.SortBy);

        // - If start/end provided => custom range (wins)
        // - Else use period if provided
        // - Else default to 30d (production-friendly dashboard behavior)
        DateTime? startUtc = null;
        DateTime? endUtc = null;

        var hasCustom = !string.IsNullOrWhiteSpace(query.StartDate) || !string.IsNullOrWhiteSpace(query.EndDate);
        var hasPeriod = !string.IsNullOrWhiteSpace(query.Period);

        if (hasCustom || hasPeriod)
        {
            var range = _dateRanges.Resolve(query.Period, query.StartDate, query.EndDate, referenceNow);

            startUtc = range.StartUtcInclusive;
            endUtc = range.EndUtcInclusive;
        }
        else
        {
            // Default to 30d if neither custom nor period is provided
            var range = _dateRanges.Resolve(period: "30d", startDate: null, endDate: null, utcNow: referenceNow);
            startUtc = range.StartUtcInclusive;
            endUtc = range.EndUtcInclusive;
        }

        IEnumerable<Models.Transaction> filtered = all;

        if (startUtc is not null && endUtc is not null)
        {
            filtered = filtered.Where(t =>
                t.Date.UtcDateTime >= startUtc.Value &&
                t.Date.UtcDateTime <= endUtc.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            var cat = query.Category.Trim();
            filtered = filtered.Where(t => string.Equals(t.Category, cat, StringComparison.OrdinalIgnoreCase));
        }

        filtered = sort switch
        {
            TransactionSortBy.DateAsc => filtered.OrderBy(t => t.Date),
            TransactionSortBy.DateDesc => filtered.OrderByDescending(t => t.Date),
            TransactionSortBy.AmountAsc => filtered.OrderBy(t => t.Amount),
            TransactionSortBy.AmountDesc => filtered.OrderByDescending(t => t.Amount),
            _ => filtered.OrderByDescending(t => t.Date)
        };

        var total = filtered.Count();

        var page = filtered
            .Skip(offset)
            .Take(limit)
            .ToList();

        var hasMore = (offset + limit) < total;

        return new TransactionsResponse(
            Transactions: page,
            Pagination: new Pagination(total, limit, offset, hasMore)
        );
    }
}
