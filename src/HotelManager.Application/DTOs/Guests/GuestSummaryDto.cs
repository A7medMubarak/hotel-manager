namespace HotelManager.Application.DTOs.Guests;

public class GuestSummaryDto
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string NationalId { get; set; }
    public string? Phone { get; set; }
    public bool IsPrimary { get; set; }
}
