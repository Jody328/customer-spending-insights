using System.Globalization;
using CustomerSpending.Api.Models;
using CustomerSpending.Api.Models.Queries;

namespace CustomerSpending.Api.Services;

public sealed class DateRangeService : IDateRangeService
{
    private static readonly HashSet<string> AllowedPeriods = new(StringComparer.OrdinalIgnoreCase)
    {
        "7d", "30d", "90d", "1y"
    };

    /// <summary>
    /// Returns the best "now" reference for period calculations.
    /// For seeded/historical demo data, this anchors "now" to the latest transaction date.
    /// Falls back to utcNow if there are no transactions.
    /// </summary>
    public DateTime GetReferenceUtcNow(IEnumerable<Transaction> transactions, DateTime utcNow)
    {
        utcNow = utcNow.Kind == DateTimeKind.Utc ? utcNow : utcNow.ToUniversalTime();

        if (transactions is null)
            return utcNow;

        // Transaction.Date is DateTimeOffset, so UtcDateTime is the safest and most explicit anchor.
        var maxUtc = transactions
            .Select(t => t.Date.UtcDateTime)
            .DefaultIfEmpty(utcNow)
            .Max();

        // Ensure the returned DateTime is UTC kind.
        return DateTime.SpecifyKind(maxUtc, DateTimeKind.Utc);
    }

    public DateRange Resolve(string? period, string? startDate, string? endDate, DateTime utcNow)
    {
        // utcNow should always be UTC. Normalize just in case.
        utcNow = utcNow.Kind == DateTimeKind.Utc ? utcNow : utcNow.ToUniversalTime();

        // If either startDate or endDate is supplied, treat as custom range (takes precedence over period).
        var hasCustom = !string.IsNullOrWhiteSpace(startDate) || !string.IsNullOrWhiteSpace(endDate);

        if (hasCustom)
        {
            var resolved = ResolveCustomRange(startDate, endDate, utcNow);
            return resolved;
        }

        // Default period
        var p = string.IsNullOrWhiteSpace(period) ? "30d" : period.Trim();

        if (!AllowedPeriods.Contains(p))
            throw new ArgumentException($"Invalid period '{p}'. Allowed: 7d, 30d, 90d, 1y");

        var endDateLocal = utcNow.Date; // today (UTC) at 00:00
        var startDateLocal = p.ToLowerInvariant() switch
        {
            "7d" => endDateLocal.AddDays(-6),
            "30d" => endDateLocal.AddDays(-29),
            "90d" => endDateLocal.AddDays(-89),
            "1y" => endDateLocal.AddDays(-364),
            _ => endDateLocal.AddDays(-29)
        };

        return CreateRange(startDateLocal, endDateLocal);
    }

    private static DateRange ResolveCustomRange(string? startDate, string? endDate, DateTime utcNow)
    {
        var hasStart = !string.IsNullOrWhiteSpace(startDate);
        var hasEnd = !string.IsNullOrWhiteSpace(endDate);

        DateTime? start = hasStart ? ParseIsoDateOrThrow(startDate!) : null;
        DateTime? end = hasEnd ? ParseIsoDateOrThrow(endDate!) : null;

        // If only one side is supplied, use a single-day range (predictable + UX friendly)
        if (start is not null && end is null) end = start.Value;
        if (end is not null && start is null) start = end.Value;

        // Fallback (shouldn't be hit when caller sets hasCustom, but safe anyway)
        start ??= utcNow.Date.AddDays(-29);
        end ??= utcNow.Date;

        if (start > end)
            throw new ArgumentException("startDate must be <= endDate");

        return CreateRange(start.Value, end.Value);
    }

    private static DateTime ParseIsoDateOrThrow(string value)
    {
        // YYYY-MM-DD
        if (!DateTime.TryParseExact(
                value.Trim(),
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var parsed))
        {
            throw new ArgumentException($"Invalid date '{value}'. Expected format YYYY-MM-DD.");
        }

        return parsed.Date;
    }

    private static DateRange CreateRange(DateTime startDateUtc, DateTime endDateUtc)
    {
        // Start of day inclusive
        var startUtc = DateTime.SpecifyKind(startDateUtc.Date, DateTimeKind.Utc);

        // End of day inclusive
        var endUtc = DateTime.SpecifyKind(endDateUtc.Date, DateTimeKind.Utc)
            .AddDays(1)
            .AddTicks(-1);

        return new DateRange(
            StartUtcInclusive: startUtc,
            EndUtcInclusive: endUtc,
            StartDate: startUtc.ToString("yyyy-MM-dd"),
            EndDate: DateTime.SpecifyKind(endDateUtc.Date, DateTimeKind.Utc).ToString("yyyy-MM-dd")
        );
    }
}
