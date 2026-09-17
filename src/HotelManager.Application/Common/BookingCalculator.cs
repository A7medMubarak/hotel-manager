using HotelManager.Domain.Entities;

namespace HotelManager.Application.Common;

public static class BookingCalculator
{
    public static int Nights(DateTime checkIn, DateTime checkOut)
    {
        var s = DateOnly.FromDateTime(checkIn);
        var e = DateOnly.FromDateTime(checkOut);
        if (s == e) return 1;
        return e.DayNumber - s.DayNumber;
    }

    public static decimal TotalCost(DateTime checkIn, DateTime checkOut, decimal pricePerNight)
        => Nights(checkIn, checkOut) * pricePerNight;

    public static decimal TotalPaid(IEnumerable<Payment> payments)
        => payments.Sum(p => p.Amount);

    public static decimal Balance(DateTime checkIn, DateTime checkOut,
        decimal pricePerNight, IEnumerable<Payment> payments)
        => TotalCost(checkIn, checkOut, pricePerNight) - TotalPaid(payments);
}
