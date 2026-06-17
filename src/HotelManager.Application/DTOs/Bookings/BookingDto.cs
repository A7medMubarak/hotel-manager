using HotelManager.Application.DTOs.Guests;
using HotelManager.Application.DTOs.Payments;

namespace HotelManager.Application.DTOs.Bookings;

public class BookingDto
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public string RoomNumber { get; set; }
    public DateOnly CheckIn { get; set; }
    public DateOnly CheckOut { get; set; }
    public decimal PricePerNight { get; set; }
    public string Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CreatedByUserId { get; set; }

    public int Nights { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal Balance { get; set; }
    public int GuestCount { get; set; }

    public List<GuestSummaryDto> Guests { get; set; } = [];
    public List<PaymentDto> Payments { get; set; } = [];
}
