using HotelManager.Application.DTOs.Bookings;
using HotelManager.Application.DTOs.Common;

namespace HotelManager.Application.Services.Interfaces;

public interface IBookingQueryService
{
    Task<List<BookingSummaryDto>> GetActiveAsync();
    Task<List<BookingSummaryDto>> GetCompletedAsync();
    Task<List<BookingSummaryDto>> GetCancelledAsync();
    Task<BookingDto> GetByIdAsync(int id);
    Task<List<BookingSummaryDto>> SearchAsync(string query);
    Task<PagedResult<BookingSummaryDto>> GetFilteredAsync(BookingFilterRequest filter);
}
