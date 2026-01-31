namespace CustomerSpending.Api.Models;

public sealed record CustomerProfile(
    string CustomerId,
    string Name,
    string Email,
    string JoinDate,
    string AccountType,
    decimal TotalSpent,
    string Currency
);
