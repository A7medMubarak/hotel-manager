using HotelManager.Domain.Entities;

namespace HotelManager.Application.Common;

public static class BookingCalculator
{
    public static int Nights(DateOnly checkIn, DateOnly checkOut)
        => checkOut.DayNumber - checkIn.DayNumber;

    public static decimal TotalCost(DateOnly checkIn, DateOnly checkOut, decimal pricePerNight)
        => Nights(checkIn, checkOut) * pricePerNight;

    public static decimal TotalPaid(IEnumerable<Payment> payments)
        => payments.Sum(p => p.Amount);

    public static decimal Balance(DateOnly checkIn, DateOnly checkOut,
        decimal pricePerNight, IEnumerable<Payment> payments)
        => TotalCost(checkIn, checkOut, pricePerNight) - TotalPaid(payments);
}
