namespace CustomerSpending.Api.Models.Queries;

public enum TransactionSortBy
{
    DateDesc,
    DateAsc,
    AmountDesc,
    AmountAsc
}

public static class TransactionSortByParser
{
    public static TransactionSortBy Parse(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return TransactionSortBy.DateDesc;

        return sortBy.Trim().ToLowerInvariant() switch
        {
            "date_desc" => TransactionSortBy.DateDesc,
            "date_asc" => TransactionSortBy.DateAsc,
            "amount_desc" => TransactionSortBy.AmountDesc,
            "amount_asc" => TransactionSortBy.AmountAsc,
            _ => throw new ArgumentException("Invalid sortBy. Allowed: date_desc, date_asc, amount_desc, amount_asc")
        };
    }
}
