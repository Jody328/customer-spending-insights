namespace CustomerSpending.Api.Models.Responses;

public sealed record SpendingSummaryResponse(
    string Period,
    decimal TotalSpent,
    int TransactionCount,
    decimal AverageTransaction,
    string TopCategory,
    ComparedToPrevious ComparedToPrevious
);

public sealed record ComparedToPrevious(
    decimal SpentChange,
    decimal TransactionChange
);
