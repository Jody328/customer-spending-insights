using CustomerSpending.Api.Models.Queries;
using CustomerSpending.Api.Models.Responses;

namespace CustomerSpending.Api.Services;

public interface ISpendingService
{
    Task<SpendingSummaryResponse> GetSummaryAsync(string customerId, string? period, CancellationToken ct = default);

    Task<SpendingByCategoryResponse> GetCategoriesAsync(string customerId, SpendingCategoriesQuery query, CancellationToken ct = default);
}
