namespace HotelManager.Application.DTOs.Bookings;

public class BookingSummaryDto
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public string RoomNumber { get; set; }
    public int PrimaryGuestId { get; set; }
    public string PrimaryGuestName { get; set; }
    public DateOnly CheckIn { get; set; }
    public DateOnly CheckOut { get; set; }
    public string Status { get; set; }
    public decimal PricePerNight { get; set; }
    public decimal TotalCost { get; set; }
    public decimal Balance { get; set; }
    public int GuestCount { get; set; }
}
