namespace HotelManager.Application.DTOs.Bookings;

public class CreateBookingRequest
{
    public int RoomId { get; set; }
    public DateOnly CheckIn { get; set; }
    public DateOnly CheckOut { get; set; }
    public decimal PricePerNight { get; set; }
    public string? Notes { get; set; }

    public int PrimaryGuestId { get; set; }
    public List<int> AdditionalGuestIds { get; set; } = [];
}
