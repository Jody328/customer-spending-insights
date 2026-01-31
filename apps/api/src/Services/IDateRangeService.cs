using CustomerSpending.Api.Models.Queries;

namespace CustomerSpending.Api.Services;

public interface IDateRangeService
{
    DateRange Resolve(string? period, string? startDate, string? endDate, DateTime utcNow);
}
