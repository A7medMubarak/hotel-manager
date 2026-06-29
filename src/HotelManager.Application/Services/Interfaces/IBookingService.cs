using HotelManager.Application.DTOs.Bookings;
using HotelManager.Application.DTOs.Common;

namespace HotelManager.Application.Services.Interfaces;

public interface IBookingService
{
    Task<List<BookingSummaryDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<List<BookingSummaryDto>> GetCompletedAsync(CancellationToken cancellationToken = default);
    Task<List<BookingSummaryDto>> GetCancelledAsync(CancellationToken cancellationToken = default);
    Task<BookingDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<BookingDto> CreateAsync(CreateBookingRequest request, int createdByUserId, CancellationToken cancellationToken = default);
    Task ExtendAsync(int id, ExtendBookingRequest request, CancellationToken cancellationToken = default);
    Task CompleteAsync(int id, CancellationToken cancellationToken = default);
    Task CancelAsync(int id, CancellationToken cancellationToken = default);
    Task<List<BookingSummaryDto>> SearchAsync(string query, CancellationToken cancellationToken = default);
    Task<PagedResult<BookingSummaryDto>> GetFilteredAsync(BookingFilterRequest filter, CancellationToken cancellationToken = default);
}
