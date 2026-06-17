using HotelManager.Application.Common;
using HotelManager.Application.DTOs.Bookings;
using HotelManager.Application.DTOs.Common;
using HotelManager.Application.DTOs.Guests;
using HotelManager.Application.Services.Interfaces;
using HotelManager.Domain.Entities;
using HotelManager.Domain.Enums;
using HotelManager.Domain.Interfaces;

namespace HotelManager.Application.Services;

public class BookingService : IBookingService
{
    private readonly IApplicationDbContext _context;

    public BookingService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<BookingSummaryDto>> GetActiveAsync()
    {
        var bookings = await _context.Bookings
            .Where(b => b.Status == BookingStatus.Active)
            .Include(b => b.Room)
            .Include(b => b.BookingGuests).ThenInclude(bg => bg.Guest)
            .Include(b => b.Payments)
            .OrderBy(b => b.CheckIn)
            .ToListAsync();

        return bookings.Select(MapToSummary).ToList();
    }

    public async Task<List<BookingSummaryDto>> GetCompletedAsync()
    {
        var bookings = await _context.Bookings
            .Where(b => b.Status == BookingStatus.Completed)
            .Include(b => b.Room)
            .Include(b => b.BookingGuests).ThenInclude(bg => bg.Guest)
            .Include(b => b.Payments)
            .OrderByDescending(b => b.CheckOut)
            .ToListAsync();

        return bookings.Select(MapToSummary).ToList();
    }

    public async Task<List<BookingSummaryDto>> GetCancelledAsync()
    {
        var bookings = await _context.Bookings
            .Where(b => b.Status == BookingStatus.Cancelled)
            .Include(b => b.Room)
            .Include(b => b.BookingGuests).ThenInclude(bg => bg.Guest)
            .Include(b => b.Payments)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return bookings.Select(MapToSummary).ToList();
    }

    public async Task<BookingDto> GetByIdAsync(int id)
    {
        var booking = await _context.Bookings
            .Include(b => b.Room)
            .Include(b => b.BookingGuests).ThenInclude(bg => bg.Guest)
            .Include(b => b.Payments)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking is null)
            throw new KeyNotFoundException($"Booking with id {id} not found.");

        return MapToDetail(booking);
    }

    public async Task<BookingDto> CreateAsync(CreateBookingRequest request, int createdByUserId)
    {
        if (request.CheckIn >= request.CheckOut)
            throw new ArgumentException("CheckIn must be before CheckOut.");

        var roomExists = await _context.Rooms.AnyAsync(r => r.Id == request.RoomId);
        if (!roomExists)
            throw new ArgumentException($"Room with id {request.RoomId} not found.");

        var guestExists = await _context.Guests.AnyAsync(g => g.Id == request.PrimaryGuestId);
        if (!guestExists)
            throw new ArgumentException($"Guest with id {request.PrimaryGuestId} not found.");

        if (request.AdditionalGuestIds.Any())
        {
            var validIds = await _context.Guests
                .CountAsync(g => request.AdditionalGuestIds.Contains(g.Id));
            if (validIds != request.AdditionalGuestIds.Count)
                throw new ArgumentException("One or more additional guest IDs are invalid.");
        }

        var available = await IsRoomAvailable(request.RoomId, request.CheckIn, request.CheckOut);
        if (!available)
            throw new ArgumentException("Room is not available for the selected dates.");

        var booking = new Booking
        {
            RoomId = request.RoomId,
            CheckIn = request.CheckIn,
            CheckOut = request.CheckOut,
            PricePerNight = request.PricePerNight,
            Status = BookingStatus.Active,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = createdByUserId
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        _context.BookingGuests.Add(new BookingGuest
        {
            BookingId = booking.Id,
            GuestId = request.PrimaryGuestId,
            IsPrimary = true
        });

        foreach (var guestId in request.AdditionalGuestIds)
        {
            _context.BookingGuests.Add(new BookingGuest
            {
                BookingId = booking.Id,
                GuestId = guestId,
                IsPrimary = false
            });
        }

        await _context.SaveChangesAsync();

        return await GetByIdAsync(booking.Id);
    }

    public async Task ExtendAsync(int id, ExtendBookingRequest request)
    {
        var booking = await _context.Bookings.FindAsync(id);

        if (booking is null)
            throw new KeyNotFoundException($"Booking with id {id} not found.");

        if (booking.Status != BookingStatus.Active)
            throw new ArgumentException("Only active bookings can be extended.");

        if (request.NewCheckOut <= booking.CheckIn)
            throw new ArgumentException("New CheckOut must be after current CheckIn.");

        if (request.NewCheckOut <= booking.CheckOut)
            throw new ArgumentException("New CheckOut must be after current CheckOut.");

        var available = await IsRoomAvailable(booking.RoomId, booking.CheckIn, request.NewCheckOut, id);
        if (!available)
            throw new ArgumentException("Room is not available for the extended period.");

        booking.CheckOut = request.NewCheckOut;
        await _context.SaveChangesAsync();
    }

    public async Task CompleteAsync(int id)
    {
        var booking = await _context.Bookings.FindAsync(id);

        if (booking is null)
            throw new KeyNotFoundException($"Booking with id {id} not found.");

        if (booking.Status != BookingStatus.Active)
            throw new ArgumentException("Only active bookings can be completed.");

        booking.Status = BookingStatus.Completed;
        await _context.SaveChangesAsync();
    }

    public async Task CancelAsync(int id)
    {
        var booking = await _context.Bookings.FindAsync(id);

        if (booking is null)
            throw new KeyNotFoundException($"Booking with id {id} not found.");

        if (booking.Status != BookingStatus.Active)
            throw new ArgumentException("Only active bookings can be cancelled.");

        booking.Status = BookingStatus.Cancelled;
        await _context.SaveChangesAsync();
    }

    public async Task<List<BookingSummaryDto>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return [];

        query = query.Trim();

        var bookings = await _context.Bookings
            .Include(b => b.Room)
            .Include(b => b.BookingGuests).ThenInclude(bg => bg.Guest)
            .Include(b => b.Payments)
            .Where(b => b.Room.Number.Contains(query) ||
                        b.BookingGuests.Any(bg => bg.Guest.FullName.Contains(query)) ||
                        b.BookingGuests.Any(bg => bg.Guest.NationalId.Contains(query)))
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return bookings.Select(MapToSummary).ToList();
    }

    public async Task<PagedResult<BookingSummaryDto>> GetFilteredAsync(BookingFilterRequest filter)
    {
        if (filter.Page < 1) filter.Page = 1;
        if (filter.PageSize < 1) filter.PageSize = 20;

        var query = _context.Bookings
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
            Items = bookings.Select(MapToSummary).ToList(),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    private async Task<bool> IsRoomAvailable(int roomId, DateOnly checkIn, DateOnly checkOut, int? excludeBookingId = null)
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

    private BookingSummaryDto MapToSummary(Booking booking)
    {
        var primaryGuest = booking.BookingGuests
            .FirstOrDefault(bg => bg.IsPrimary);

        var payments = booking.Payments.ToList();
        var totalCost = BookingCalculator.TotalCost(booking.CheckIn, booking.CheckOut, booking.PricePerNight);
        var totalPaid = BookingCalculator.TotalPaid(payments);
        var balance = BookingCalculator.Balance(booking.CheckIn, booking.CheckOut, booking.PricePerNight, payments);

        return new BookingSummaryDto
        {
            Id = booking.Id,
            RoomId = booking.RoomId,
            RoomNumber = booking.Room.Number,
            PrimaryGuestId = primaryGuest?.Guest.Id ?? 0,
            PrimaryGuestName = primaryGuest?.Guest.FullName ?? "N/A",
            CheckIn = booking.CheckIn,
            CheckOut = booking.CheckOut,
            Status = booking.Status.ToString(),
            PricePerNight = booking.PricePerNight,
            TotalCost = totalCost,
            Balance = balance,
            GuestCount = booking.BookingGuests.Count
        };
    }

    private BookingDto MapToDetail(Booking booking)
    {
        var payments = booking.Payments.ToList();
        var nights = BookingCalculator.Nights(booking.CheckIn, booking.CheckOut);
        var totalCost = BookingCalculator.TotalCost(booking.CheckIn, booking.CheckOut, booking.PricePerNight);
        var totalPaid = BookingCalculator.TotalPaid(payments);
        var balance = BookingCalculator.Balance(booking.CheckIn, booking.CheckOut, booking.PricePerNight, payments);

        return new BookingDto
        {
            Id = booking.Id,
            RoomId = booking.RoomId,
            RoomNumber = booking.Room.Number,
            CheckIn = booking.CheckIn,
            CheckOut = booking.CheckOut,
            PricePerNight = booking.PricePerNight,
            Status = booking.Status.ToString(),
            Notes = booking.Notes,
            CreatedAt = booking.CreatedAt,
            CreatedByUserId = booking.CreatedByUserId,
            Nights = nights,
            TotalCost = totalCost,
            TotalPaid = totalPaid,
            Balance = balance,
            GuestCount = booking.BookingGuests.Count,
            Guests = booking.BookingGuests.Select(bg => new GuestSummaryDto
            {
                Id = bg.Guest.Id,
                FullName = bg.Guest.FullName,
                NationalId = bg.Guest.NationalId,
                Phone = bg.Guest.Phone,
                IsPrimary = bg.IsPrimary
            }).ToList(),
            Payments = payments.Select(p => new DTOs.Payments.PaymentDto
            {
                Id = p.Id,
                BookingId = p.BookingId,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                Notes = p.Notes,
                CreatedByUserId = p.CreatedByUserId,
                CreatedAt = p.CreatedAt
            }).ToList()
        };
    }
}
