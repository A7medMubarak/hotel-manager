using HotelManager.Application.Services.Interfaces;
using HotelManager.Domain.Enums;
using HotelManager.Domain.Interfaces;

namespace HotelManager.Application.Services;

public class BookingAvailabilityService : IBookingAvailabilityService
{
    private readonly IApplicationDbContext _context;

    public BookingAvailabilityService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsRoomAvailable(int roomId, DateOnly checkIn, DateOnly checkOut, int? excludeBookingId = null)
    {
        var query = _context.Bookings.Where(b =>
            b.RoomId == roomId &&
            b.Status == BookingStatus.Active &&
            b.CheckIn < checkOut &&
            b.CheckOut > checkIn);

        if (excludeBookingId.HasValue)
            query = query.Where(b => b.Id != excludeBookingId.Value);

        return !await query.AnyAsync();
    }
}
