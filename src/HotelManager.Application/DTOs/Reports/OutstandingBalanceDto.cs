namespace HotelManager.Application.DTOs.Reports;

public class OutstandingBalanceDto
{
    public int BookingId { get; set; }
    public string RoomNumber { get; set; }
    public string GuestName { get; set; }
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal Balance { get; set; }
}
