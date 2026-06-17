namespace HotelManager.Application.Common;

public static class BusinessDateHelper
{
    public static DateOnly GetBusinessDate()
        => DateTime.Now.Hour >= 12
            ? DateOnly.FromDateTime(DateTime.Today)
            : DateOnly.FromDateTime(DateTime.Today.AddDays(-1));

    public static (DateTime Start, DateTime End) GetBusinessDayWindow()
    {
        var d = GetBusinessDate();
        return (
            d.ToDateTime(new TimeOnly(12, 0)),
            d.AddDays(1).ToDateTime(new TimeOnly(12, 0))
        );
    }
}
