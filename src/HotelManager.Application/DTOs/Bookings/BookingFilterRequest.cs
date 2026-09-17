namespace HotelManager.Application.DTOs.Bookings;

public class BookingFilterRequest
{
    public string? Status { get; set; }
    public DateTime? CheckInFrom { get; set; }
    public DateTime? CheckInTo { get; set; }
    public DateTime? CheckOutFrom { get; set; }
    public DateTime? CheckOutTo { get; set; }
    public string? RoomNumber { get; set; }
    public string? GuestName { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
