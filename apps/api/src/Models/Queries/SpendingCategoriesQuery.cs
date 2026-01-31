namespace CustomerSpending.Api.Models.Queries;

public sealed record SpendingCategoriesQuery(
    string? Period,
    string? StartDate,
    string? EndDate
);
