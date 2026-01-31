namespace CustomerSpending.Api.Models.Queries;

public sealed record TransactionsQuery(
    int Limit,
    int Offset,
    string? Category,
    string? Period,
    string? StartDate,
    string? EndDate,
    string? SortBy
);
