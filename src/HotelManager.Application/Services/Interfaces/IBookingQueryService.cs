using HotelManager.Application.DTOs.Bookings;
using HotelManager.Application.DTOs.Common;

namespace HotelManager.Application.Services.Interfaces;

public interface IBookingQueryService
{
    Task<List<BookingSummaryDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<List<BookingSummaryDto>> GetCompletedAsync(CancellationToken cancellationToken = default);
    Task<List<BookingSummaryDto>> GetCancelledAsync(CancellationToken cancellationToken = default);
    Task<BookingDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<BookingSummaryDto>> SearchAsync(string query, CancellationToken cancellationToken = default);
    Task<PagedResult<BookingSummaryDto>> GetFilteredAsync(BookingFilterRequest filter, CancellationToken cancellationToken = default);
}
