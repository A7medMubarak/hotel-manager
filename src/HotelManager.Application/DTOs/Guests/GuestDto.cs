namespace HotelManager.Application.DTOs.Guests;

public class GuestDto
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string NationalId { get; set; }
    public string Address { get; set; }
    public string? Phone { get; set; }
    public DateTime CreatedAt { get; set; }
}
