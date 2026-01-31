namespace CustomerSpending.Api.Models;

public sealed record MonthlyTrend(
    string Month,
    decimal TotalSpent,
    int TransactionCount,
    decimal AverageTransaction
);
