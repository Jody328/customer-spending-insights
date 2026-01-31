namespace CustomerSpending.Api.Models.Queries;

/// <summary>
/// Resolved UTC date range for filtering transactions.
/// </summary>
public sealed record DateRange(DateTime StartUtcInclusive, DateTime EndUtcInclusive, string StartDate, string EndDate);
