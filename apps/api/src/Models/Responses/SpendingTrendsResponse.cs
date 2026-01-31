using CustomerSpending.Api.Models;

namespace CustomerSpending.Api.Models.Responses;

public sealed record SpendingTrendsResponse(IReadOnlyList<MonthlyTrend> Trends);
