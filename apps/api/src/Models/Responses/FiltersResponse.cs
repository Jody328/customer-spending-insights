namespace CustomerSpending.Api.Models.Responses;

public sealed record FiltersResponse(
    IReadOnlyList<CategoryFilter> Categories,
    IReadOnlyList<DateRangePreset> DateRangePresets
);

public sealed record CategoryFilter(string Name, string Color, string Icon);

public sealed record DateRangePreset(string Label, string Value);
