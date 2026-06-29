using HotelManager.Application.Common;
using HotelManager.Application.DTOs.Bookings;
using HotelManager.Application.DTOs.Common;
using HotelManager.Application.Services.Interfaces;
using HotelManager.Domain.Entities;
using HotelManager.Domain.Enums;
using HotelManager.Domain.Interfaces;

namespace HotelManager.Application.Services;

public class BookingQueryService : IBookingQueryService
{
    private readonly IApplicationDbContext _context;

    public BookingQueryService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<BookingSummaryDto>> GetActiveAsync()
    {
        var bookings = await _context.Bookings
            .AsNoTracking()
            .Where(b => b.Status == BookingStatus.Active)
            .Include(b => b.Room)
            .Include(b => b.BookingGuests).ThenInclude(bg => bg.Guest)
            .Include(b => b.Payments)
            .OrderBy(b => b.CheckIn)
            .ToListAsync();

        return bookings.Select(b => b.ToSummaryDto()).ToList();
    }

    public async Task<List<BookingSummaryDto>> GetCompletedAsync()
    {
        var bookings = await _context.Bookings
            .AsNoTracking()
            .Where(b => b.Status == BookingStatus.Completed)
            .Include(b => b.Room)
            .Include(b => b.BookingGuests).ThenInclude(bg => bg.Guest)
            .Include(b => b.Payments)
            .OrderByDescending(b => b.CheckOut)
            .ToListAsync();

        return bookings.Select(b => b.ToSummaryDto()).ToList();
    }

    public async Task<List<BookingSummaryDto>> GetCancelledAsync()
    {
        var bookings = await _context.Bookings
            .AsNoTracking()
            .Where(b => b.Status == BookingStatus.Cancelled)
            .Include(b => b.Room)
            .Include(b => b.BookingGuests).ThenInclude(bg => bg.Guest)
            .Include(b => b.Payments)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return bookings.Select(b => b.ToSummaryDto()).ToList();
    }

    public async Task<BookingDto> GetByIdAsync(int id)
    {
        var booking = await _context.Bookings
            .AsNoTracking()
            .Include(b => b.Room)
            .Include(b => b.BookingGuests).ThenInclude(bg => bg.Guest)
            .Include(b => b.Payments)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking is null)
            throw new KeyNotFoundException($"Booking with id {id} not found.");

        return booking.ToDetailDto();
    }

    public async Task<List<BookingSummaryDto>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return [];

        query = query.Trim();

        var bookings = await _context.Bookings
            .AsNoTracking()
            .Include(b => b.Room)
            .Include(b => b.BookingGuests).ThenInclude(bg => bg.Guest)
            .Include(b => b.Payments)
            .Where(b => b.Room.Number.Contains(query) ||
                        b.BookingGuests.Any(bg => bg.Guest.FullName.Contains(query)) ||
                        b.BookingGuests.Any(bg => bg.Guest.NationalId.Contains(query)))
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return bookings.Select(b => b.ToSummaryDto()).ToList();
    }

    public async Task<PagedResult<BookingSummaryDto>> GetFilteredAsync(BookingFilterRequest filter)
    {
        if (filter.Page < 1) filter.Page = 1;
        if (filter.PageSize < 1) filter.PageSize = 20;

        var query = _context.Bookings
            .AsNoTracking()
            .Include(b => b.Room)
            .Include(b => b.BookingGuests).ThenInclude(bg => bg.Guest)
            .Include(b => b.Payments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            if (Enum.TryParse<BookingStatus>(filter.Status, true, out var status))
                query = query.Where(b => b.Status == status);
        }

        if (filter.CheckInFrom.HasValue)
            query = query.Where(b => b.CheckIn >= filter.CheckInFrom.Value);

        if (filter.CheckInTo.HasValue)
            query = query.Where(b => b.CheckIn <= filter.CheckInTo.Value);

        if (filter.CheckOutFrom.HasValue)
            query = query.Where(b => b.CheckOut >= filter.CheckOutFrom.Value);

        if (filter.CheckOutTo.HasValue)
            query = query.Where(b => b.CheckOut <= filter.CheckOutTo.Value);

        if (!string.IsNullOrWhiteSpace(filter.RoomNumber))
            query = query.Where(b => b.Room.Number.Contains(filter.RoomNumber));

        if (!string.IsNullOrWhiteSpace(filter.GuestName))
            query = query.Where(b => b.BookingGuests.Any(bg => bg.Guest.FullName.Contains(filter.GuestName)));

        var totalCount = await query.CountAsync();

        var bookings = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<BookingSummaryDto>
        {
            Items = bookings.Select(b => b.ToSummaryDto()).ToList(),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }
}
