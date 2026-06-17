namespace HotelManager.Application.DTOs.Rooms;

public class RoomFilterRequest
{
    public string? Status { get; set; }
    public int? Floor { get; set; }
    public int? MinBedCount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
