using CustomerSpending.Api.Services;
using Xunit;

namespace CustomerSpending.Api.Tests.Services;

public class DateRangeServiceTests
{
    private readonly DateRangeService _sut = new();

    [Fact]
    public void Resolve_DefaultPeriod_Is30d()
    {
        var now = new DateTime(2024, 09, 16, 12, 0, 0, DateTimeKind.Utc);

        var range = _sut.Resolve(period: null, startDate: null, endDate: null, utcNow: now);

        Assert.Equal("2024-08-18", range.StartDate);
        Assert.Equal("2024-09-16", range.EndDate);
    }

    [Theory]
    [InlineData("7d", "2024-09-10", "2024-09-16")]
    [InlineData("90d", "2024-06-19", "2024-09-16")]
    [InlineData("1y", "2023-09-18", "2024-09-16")]
    public void Resolve_Periods_Work(string period, string expectedStart, string expectedEnd)
    {
        var now = new DateTime(2024, 09, 16, 12, 0, 0, DateTimeKind.Utc);

        var range = _sut.Resolve(period, startDate: null, endDate: null, utcNow: now);

        Assert.Equal(expectedStart, range.StartDate);
        Assert.Equal(expectedEnd, range.EndDate);
    }

    [Fact]
    public void Resolve_CustomRange_TakesPrecedenceOverPeriod()
    {
        var now = new DateTime(2024, 09, 16, 12, 0, 0, DateTimeKind.Utc);

        var range = _sut.Resolve(period: "7d", startDate: "2024-09-01", endDate: "2024-09-10", utcNow: now);

        Assert.Equal("2024-09-01", range.StartDate);
        Assert.Equal("2024-09-10", range.EndDate);
    }

    [Fact]
    public void Resolve_CustomRange_MissingEndDate_DefaultsToToday()
    {
        var now = new DateTime(2024, 09, 16, 12, 0, 0, DateTimeKind.Utc);

        var range = _sut.Resolve(period: null, startDate: "2024-09-01", endDate: null, utcNow: now);

        Assert.Equal("2024-09-01", range.StartDate);
        Assert.Equal("2024-09-16", range.EndDate);
    }

    [Fact]
    public void Resolve_CustomRange_MissingStartDate_DefaultsTo30dBeforeEnd()
    {
        var now = new DateTime(2024, 09, 16, 12, 0, 0, DateTimeKind.Utc);

        var range = _sut.Resolve(period: null, startDate: null, endDate: "2024-09-16", utcNow: now);

        Assert.Equal("2024-08-18", range.StartDate);
        Assert.Equal("2024-09-16", range.EndDate);
    }

    [Fact]
    public void Resolve_InvalidPeriod_Throws()
    {
        var now = new DateTime(2024, 09, 16, 12, 0, 0, DateTimeKind.Utc);

        Assert.Throws<ArgumentException>(() =>
            _sut.Resolve("14d", null, null, now));
    }

    [Fact]
    public void Resolve_InvalidDate_Throws()
    {
        var now = new DateTime(2024, 09, 16, 12, 0, 0, DateTimeKind.Utc);

        Assert.Throws<ArgumentException>(() =>
            _sut.Resolve(null, "2024/09/01", "2024-09-10", now));
    }

    [Fact]
    public void Resolve_StartAfterEnd_Throws()
    {
        var now = new DateTime(2024, 09, 16, 12, 0, 0, DateTimeKind.Utc);

        Assert.Throws<ArgumentException>(() =>
            _sut.Resolve(null, "2024-09-10", "2024-09-01", now));
    }
}
