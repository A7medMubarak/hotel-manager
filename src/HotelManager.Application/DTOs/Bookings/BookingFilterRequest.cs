namespace HotelManager.Application.DTOs.Bookings;

public class BookingFilterRequest
{
    public string? Status { get; set; }
    public DateOnly? CheckInFrom { get; set; }
    public DateOnly? CheckInTo { get; set; }
    public DateOnly? CheckOutFrom { get; set; }
    public DateOnly? CheckOutTo { get; set; }
    public string? RoomNumber { get; set; }
    public string? GuestName { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
