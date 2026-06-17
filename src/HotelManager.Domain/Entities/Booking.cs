using HotelManager.Domain.Enums;

namespace HotelManager.Domain.Entities;

public class Booking
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public DateOnly CheckIn { get; set; }
    public DateOnly CheckOut { get; set; }
    public decimal PricePerNight { get; set; }
    public BookingStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CreatedByUserId { get; set; }

    public Room Room { get; set; }
    public User CreatedBy { get; set; }
    public ICollection<BookingGuest> BookingGuests { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
}
