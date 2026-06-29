namespace HotelManager.Application.Services.Interfaces;

public interface IBookingAvailabilityService
{
    Task<bool> IsRoomAvailable(int roomId, DateOnly checkIn, DateOnly checkOut, int? excludeBookingId = null, CancellationToken cancellationToken = default);
}
