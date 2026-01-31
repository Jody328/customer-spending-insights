using CustomerSpending.Api.Models.Queries;
using CustomerSpending.Api.Repositories;
using CustomerSpending.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CustomerSpending.Api.Endpoints;

public static class CustomersEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/customers/{customerId}")
            .WithTags("Customers");

        group.MapGet("/spending/summary", async (
            [FromRoute] string customerId,
            [FromQuery] string? period,
            ISpendingService spending,
            CancellationToken ct) =>
        {
            try
            {
                var result = await spending.GetSummaryAsync(customerId, period, ct);
                return Results.Ok(result);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapGet("/spending/categories", async (
            [FromRoute] string customerId,
            [FromQuery] string? period,
            [FromQuery] string? startDate,
            [FromQuery] string? endDate,
            ISpendingService spending,
            CancellationToken ct) =>
        {
            try
            {
                var query = new SpendingCategoriesQuery(period, startDate, endDate);
                var result = await spending.GetCategoriesAsync(customerId, query, ct);
                return Results.Ok(result);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapGet("/transactions", async (
            [FromRoute] string customerId,
            [FromQuery] int? limit,
            [FromQuery] int? offset,
            [FromQuery] string? category,
            [FromQuery] string? period,
            [FromQuery] string? startDate,
            [FromQuery] string? endDate,
            [FromQuery] string? sortBy,
            Services.ITransactionsService transactionsService,
            CancellationToken ct) =>
        {
            try
            {
                var query = new Models.Queries.TransactionsQuery(
                    Limit: limit ?? 20,
                    Offset: offset ?? 0,
                    Category: category,
                    Period: period,
                    StartDate: startDate,
                    EndDate: endDate,
                    SortBy: sortBy
                );

                var result = await transactionsService.GetTransactionsAsync(customerId, query, ct);
                return Results.Ok(result);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });


        group.MapGet("/profile", async (
            [FromRoute] string customerId,
            ICustomerRepository customers,
            CancellationToken ct) =>
        {
            var profile = await customers.GetProfileAsync(customerId, ct);
            return profile is null ? Results.NotFound() : Results.Ok(profile);
        });

        group.MapGet("/filters", async (
            [FromRoute] string customerId,
            ICustomerRepository customers,
            CancellationToken ct) =>
        {
            var filters = await customers.GetFiltersAsync(customerId, ct);
            return filters is null ? Results.NotFound() : Results.Ok(filters);
        });

        group.MapGet("/goals", async (
            [FromRoute] string customerId,
            ICustomerRepository customers,
            CancellationToken ct) =>
        {
            var goals = await customers.GetGoalsAsync(customerId, ct);
            return goals is null ? Results.NotFound() : Results.Ok(goals);
        });

        group.MapGet("/spending/trends", async (
            [FromRoute] string customerId,
            [FromQuery] int? months,
            ICustomerRepository customers,
            CancellationToken ct) =>
        {
            var m = months ?? 12;
            if (m < 1 || m > 24) return Results.BadRequest(new { error = "months must be between 1 and 24" });

            var trends = await customers.GetTrendsAsync(customerId, m, ct);
            return trends is null ? Results.NotFound() : Results.Ok(trends);
        });
    }
}
