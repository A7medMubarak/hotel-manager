using HotelManager.Domain.Enums;

namespace HotelManager.Domain.Entities;

public class Room
{
    public int Id { get; set; }
    public string Number { get; set; }
    public int Floor { get; set; }
    public int BedCount { get; set; }
    public BathroomType BathroomType { get; set; }
    public decimal BasePricePerNight { get; set; }
    public string? Notes { get; set; }
    public bool IsUnderMaintenance { get; set; }

    public ICollection<Booking> Bookings { get; set; } = [];
}
