namespace HotelManager.Application.DTOs.Guests;

public class GuestFilterRequest
{
    public string? Search { get; set; }
    public string? Phone { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
