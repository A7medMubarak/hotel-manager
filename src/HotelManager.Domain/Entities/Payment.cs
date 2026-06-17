namespace HotelManager.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Notes { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Booking Booking { get; set; }
    public User CreatedBy { get; set; }
}
