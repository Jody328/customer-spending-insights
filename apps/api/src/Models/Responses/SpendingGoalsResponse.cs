namespace CustomerSpending.Api.Models.Responses;

public sealed record SpendingGoalsResponse(IReadOnlyList<SpendingGoal> Goals);

public sealed record SpendingGoal(
    string Id,
    string Category,
    decimal MonthlyBudget,
    decimal CurrentSpent,
    decimal PercentageUsed,
    int DaysRemaining,
    string Status
);
