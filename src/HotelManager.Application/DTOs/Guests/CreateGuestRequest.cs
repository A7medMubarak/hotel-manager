namespace HotelManager.Application.DTOs.Guests;

public class CreateGuestRequest
{
    public string FullName { get; set; }
    public string NationalId { get; set; }
    public string Address { get; set; }
    public string? Phone { get; set; }
}
