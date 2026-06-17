namespace HotelManager.Application.DTOs.Rooms;

public class UpdateRoomRequest
{
    public string Number { get; set; }
    public int Floor { get; set; }
    public int BedCount { get; set; }
    public int BathroomType { get; set; }
    public decimal BasePricePerNight { get; set; }
    public string? Notes { get; set; }
}
