using CustomerSpending.Api.Models;

namespace CustomerSpending.Api.Models.Responses;

public sealed record TransactionsResponse(
    IReadOnlyList<Transaction> Transactions,
    Pagination Pagination
);

public sealed record Pagination(
    int Total,
    int Limit,
    int Offset,
    bool HasMore
);
