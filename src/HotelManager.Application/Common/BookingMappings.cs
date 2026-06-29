using HotelManager.Application.DTOs.Bookings;
using HotelManager.Application.DTOs.Guests;
using HotelManager.Domain.Entities;

namespace HotelManager.Application.Common;

public static class BookingMappings
{
    public static BookingSummaryDto ToSummaryDto(this Booking booking)
    {
        var primaryGuest = booking.BookingGuests
            .FirstOrDefault(bg => bg.IsPrimary);

        var payments = booking.Payments.ToList();
        var totalCost = BookingCalculator.TotalCost(booking.CheckIn, booking.CheckOut, booking.PricePerNight);
        var totalPaid = BookingCalculator.TotalPaid(payments);
        var balance = BookingCalculator.Balance(booking.CheckIn, booking.CheckOut, booking.PricePerNight, payments);

        return new BookingSummaryDto
        {
            Id = booking.Id,
            RoomId = booking.RoomId,
            RoomNumber = booking.Room.Number,
            PrimaryGuestId = primaryGuest?.Guest.Id ?? 0,
            PrimaryGuestName = primaryGuest?.Guest.FullName ?? "N/A",
            CheckIn = booking.CheckIn,
            CheckOut = booking.CheckOut,
            Status = booking.Status.ToString(),
            PricePerNight = booking.PricePerNight,
            TotalCost = totalCost,
            Balance = balance,
            GuestCount = booking.BookingGuests.Count
        };
    }

    public static BookingDto ToDetailDto(this Booking booking)
    {
        var payments = booking.Payments.ToList();
        var nights = BookingCalculator.Nights(booking.CheckIn, booking.CheckOut);
        var totalCost = BookingCalculator.TotalCost(booking.CheckIn, booking.CheckOut, booking.PricePerNight);
        var totalPaid = BookingCalculator.TotalPaid(payments);
        var balance = BookingCalculator.Balance(booking.CheckIn, booking.CheckOut, booking.PricePerNight, payments);

        return new BookingDto
        {
            Id = booking.Id,
            RoomId = booking.RoomId,
            RoomNumber = booking.Room.Number,
            CheckIn = booking.CheckIn,
            CheckOut = booking.CheckOut,
            PricePerNight = booking.PricePerNight,
            Status = booking.Status.ToString(),
            Notes = booking.Notes,
            CreatedAt = booking.CreatedAt,
            CreatedByUserId = booking.CreatedByUserId,
            Nights = nights,
            TotalCost = totalCost,
            TotalPaid = totalPaid,
            Balance = balance,
            GuestCount = booking.BookingGuests.Count,
            Guests = booking.BookingGuests.Select(bg => new GuestSummaryDto
            {
                Id = bg.Guest.Id,
                FullName = bg.Guest.FullName,
                NationalId = bg.Guest.NationalId,
                Phone = bg.Guest.Phone,
                IsPrimary = bg.IsPrimary
            }).ToList(),
            Payments = payments.Select(p => new DTOs.Payments.PaymentDto
            {
                Id = p.Id,
                BookingId = p.BookingId,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                Notes = p.Notes,
                CreatedByUserId = p.CreatedByUserId,
                CreatedAt = p.CreatedAt
            }).ToList()
        };
    }
}
