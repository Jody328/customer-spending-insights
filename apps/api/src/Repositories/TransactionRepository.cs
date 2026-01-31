using CustomerSpending.Api.Data;
using CustomerSpending.Api.Models;

namespace CustomerSpending.Api.Repositories;

public sealed class TransactionRepository : ITransactionRepository
{
    private readonly InMemoryDataStore _store;

    public TransactionRepository(InMemoryDataStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<Transaction>> GetAllAsync(string customerId, CancellationToken ct = default)
    {
        // In a real system, transactions would be filtered by customerId.
        // For this demo, I only seeded one customer's transactions.
        return Task.FromResult<IReadOnlyList<Transaction>>(_store.Transactions);
    }
}
