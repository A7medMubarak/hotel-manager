namespace HotelManager.Application.DTOs.Payments;

public class AddPaymentRequest
{
    public int BookingId { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
}
