using HotelManager.Application.DTOs.Bookings;
using HotelManager.Application.DTOs.Common;

namespace HotelManager.Application.Services.Interfaces;

public interface IBookingService
{
    Task<List<BookingSummaryDto>> GetActiveAsync();
    Task<List<BookingSummaryDto>> GetCompletedAsync();
    Task<List<BookingSummaryDto>> GetCancelledAsync();
    Task<BookingDto> GetByIdAsync(int id);
    Task<BookingDto> CreateAsync(CreateBookingRequest request, int createdByUserId);
    Task ExtendAsync(int id, ExtendBookingRequest request);
    Task CompleteAsync(int id);
    Task CancelAsync(int id);
    Task<List<BookingSummaryDto>> SearchAsync(string query);
    Task<PagedResult<BookingSummaryDto>> GetFilteredAsync(BookingFilterRequest filter);
}
