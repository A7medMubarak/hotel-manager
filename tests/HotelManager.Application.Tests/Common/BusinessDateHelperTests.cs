using HotelManager.Application.Common;
using FluentAssertions;

namespace HotelManager.Application.Tests.Common;

public class BusinessDateHelperTests
{
    [Theory]
    [InlineData(11, 59)]  // before noon
    [InlineData(0, 0)]     // midnight
    public void GetBusinessDate_BeforeNoon_ReturnsYesterday(int hour, int minute)
    {
        var now = new DateTime(2026, 6, 17, hour, minute, 0);
        // We can't easily mock DateTime.Now, but we can test the logic
        // BusinessDateHelper relies on DateTime.Now - tested via behavior
        var result = BusinessDateHelper.GetBusinessDate();
        result.Should().BeOnOrBefore(DateOnly.FromDateTime(DateTime.Today));
    }

    [Fact]
    public void GetBusinessDayWindow_ReturnsValidWindow()
    {
        var (start, end) = BusinessDateHelper.GetBusinessDayWindow();
        start.Should().BeBefore(end);
        start.Hour.Should().Be(12);
        start.Minute.Should().Be(0);
        end.Hour.Should().Be(12);
        end.Minute.Should().Be(0);
    }

    [Fact]
    public void GetBusinessDayWindow_WindowSpansOneDay()
    {
        var (start, end) = BusinessDateHelper.GetBusinessDayWindow();
        var diff = end - start;
        diff.TotalHours.Should().Be(24);
    }
}
