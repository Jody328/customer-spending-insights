using CustomerSpending.Api.Models.Queries;
using CustomerSpending.Api.Models.Responses;

namespace CustomerSpending.Api.Services;

public interface ITransactionsService
{
    Task<TransactionsResponse> GetTransactionsAsync(string customerId, TransactionsQuery query, CancellationToken ct = default);
}
