namespace HotelManager.Domain.Entities;

public class BookingNight
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int RoomId { get; set; }
    public DateOnly Date { get; set; }

    public Booking Booking { get; set; } = null!;
    public Room Room { get; set; } = null!;
}
