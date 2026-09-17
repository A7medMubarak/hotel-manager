namespace HotelManager.Application.Common;

public static class BusinessDateHelper
{
    public static DateTime GetBusinessDate()
        => DateTime.Now.Hour >= 12
            ? DateTime.UtcNow.Date
            : DateTime.UtcNow.Date.AddDays(-1);

    public static (DateTime Start, DateTime End) GetBusinessDayWindow()
    {
        var d = GetBusinessDate();
        return (d.AddHours(12), d.AddDays(1).AddHours(12));
    }

    public static List<DateOnly> GetNightDates(DateTime checkIn, DateTime checkOut)
    {
        var s = DateOnly.FromDateTime(checkIn);
        var e = DateOnly.FromDateTime(checkOut);
        if (s == e) return new List<DateOnly> { s };
        var result = new List<DateOnly>();
        for (var d = s; d < e; d = d.AddDays(1)) result.Add(d);
        return result;
    }
}
