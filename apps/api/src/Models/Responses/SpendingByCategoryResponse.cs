namespace CustomerSpending.Api.Models.Responses;

public sealed record SpendingByCategoryResponse(
    DateRangeDto DateRange,
    decimal TotalAmount,
    IReadOnlyList<CategorySpendDto> Categories
);

public sealed record DateRangeDto(string StartDate, string EndDate);

public sealed record CategorySpendDto(
    string Name,
    decimal Amount,
    decimal Percentage,
    int TransactionCount,
    string Color,
    string Icon
);
