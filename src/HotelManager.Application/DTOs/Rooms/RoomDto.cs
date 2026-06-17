namespace HotelManager.Application.DTOs.Rooms;

public class RoomDto
{
    public int Id { get; set; }
    public string Number { get; set; }
    public int Floor { get; set; }
    public int BedCount { get; set; }
    public string BathroomType { get; set; }
    public decimal BasePricePerNight { get; set; }
    public string? Notes { get; set; }
    public bool IsUnderMaintenance { get; set; }
    public string Status { get; set; }
}
