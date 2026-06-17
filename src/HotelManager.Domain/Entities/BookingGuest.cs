namespace HotelManager.Domain.Entities;

public class BookingGuest
{
    public int BookingId { get; set; }
    public int GuestId { get; set; }
    public bool IsPrimary { get; set; }

    public Booking Booking { get; set; }
    public Guest Guest { get; set; }
}
