namespace HotelManager.Application.Services.Interfaces;

public interface IBookingAvailabilityService
{
    Task<bool> IsRoomAvailable(int roomId, DateTime checkIn, DateTime checkOut, int? excludeBookingId = null, CancellationToken cancellationToken = default);
}
