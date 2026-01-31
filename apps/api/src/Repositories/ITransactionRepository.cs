using CustomerSpending.Api.Models;

namespace CustomerSpending.Api.Repositories;

public interface ITransactionRepository
{
    Task<IReadOnlyList<Transaction>> GetAllAsync(string customerId, CancellationToken ct = default);
}
