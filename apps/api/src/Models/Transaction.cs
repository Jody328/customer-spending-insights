namespace CustomerSpending.Api.Models;

public sealed record Transaction(
    string Id,
    DateTimeOffset Date,
    string Merchant,
    string Category,
    decimal Amount,
    string Description,
    string PaymentMethod,
    string Icon,
    string CategoryColor
);
