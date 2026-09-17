using HotelManager.Application.Common;
using HotelManager.Application.DTOs.Bookings;
using HotelManager.Application.DTOs.Common;
using HotelManager.Application.Services.Interfaces;
using HotelManager.Domain.Entities;
using HotelManager.Domain.Enums;
using HotelManager.Domain.Exceptions;
using HotelManager.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.Application.Services;

public class BookingService : IBookingService
{
    private readonly IApplicationDbContext _context;
    private readonly IBookingQueryService _queryService;
    private readonly IBookingAvailabilityService _availabilityService;

    public BookingService(
        IApplicationDbContext context,
        IBookingQueryService queryService,
        IBookingAvailabilityService availabilityService)
    {
        _context = context;
        _queryService = queryService;
        _availabilityService = availabilityService;
    }

    public async Task<List<BookingSummaryDto>> GetActiveAsync(CancellationToken cancellationToken = default)
        => await _queryService.GetActiveAsync(cancellationToken);

    public async Task<List<BookingSummaryDto>> GetCompletedAsync(CancellationToken cancellationToken = default)
        => await _queryService.GetCompletedAsync(cancellationToken);

    public async Task<List<BookingSummaryDto>> GetCancelledAsync(CancellationToken cancellationToken = default)
        => await _queryService.GetCancelledAsync(cancellationToken);

    public async Task<BookingDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _queryService.GetByIdAsync(id, cancellationToken);

    public async Task<List<BookingSummaryDto>> SearchAsync(string query, CancellationToken cancellationToken = default)
        => await _queryService.SearchAsync(query, cancellationToken);

    public async Task<PagedResult<BookingSummaryDto>> GetFilteredAsync(BookingFilterRequest filter, CancellationToken cancellationToken = default)
        => await _queryService.GetFilteredAsync(filter, cancellationToken);

    public async Task<BookingDto> CreateAsync(CreateBookingRequest request, int createdByUserId, CancellationToken cancellationToken = default)
    {
        if (request.CheckIn >= request.CheckOut)
            throw new ArgumentException("CheckIn must be before CheckOut.");

        var room = await _context.Rooms.FindAsync(new object[] { request.RoomId }, cancellationToken);
        if (room is null)
            throw new ArgumentException($"Room with id {request.RoomId} not found.");
        if (room.IsUnderMaintenance)
            throw new ArgumentException("Room is under maintenance and cannot be booked.");

        var guestExists = await _context.Guests.AnyAsync(g => g.Id == request.PrimaryGuestId, cancellationToken);
        if (!guestExists)
            throw new ArgumentException($"Guest with id {request.PrimaryGuestId} not found.");

        if (request.AdditionalGuestIds.Any())
        {
            var validIds = await _context.Guests
                .CountAsync(g => request.AdditionalGuestIds.Contains(g.Id), cancellationToken);
            if (validIds != request.AdditionalGuestIds.Count)
                throw new ArgumentException("One or more additional guest IDs are invalid.");
        }

        var available = await _availabilityService.IsRoomAvailable(request.RoomId, request.CheckIn, request.CheckOut, cancellationToken: cancellationToken);
        if (!available)
            throw new RoomNotAvailableException("Room is not available for the selected dates.");

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

        booking.BookingGuests.Add(new BookingGuest
        {
            GuestId = request.PrimaryGuestId,
            IsPrimary = true
        });

        foreach (var guestId in request.AdditionalGuestIds)
        {
            booking.BookingGuests.Add(new BookingGuest
            {
                GuestId = guestId,
                IsPrimary = false
            });
        }

        var nightDates = BusinessDateHelper.GetNightDates(booking.CheckIn, booking.CheckOut);
        foreach (var date in nightDates)
            booking.BookingNights.Add(new BookingNight { RoomId = booking.RoomId, Date = date });

        _context.Bookings.Add(booking);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            throw new RoomNotAvailableException("Room is not available for the selected dates.");
        }

        return await _queryService.GetByIdAsync(booking.Id, cancellationToken);
    }

    public async Task ExtendAsync(int id, ExtendBookingRequest request, CancellationToken cancellationToken = default)
    {
        var booking = await _context.Bookings
            .Include(b => b.BookingNights)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (booking is null)
            throw new KeyNotFoundException($"Booking with id {id} not found.");

        if (booking.Status != BookingStatus.Active)
            throw new ArgumentException("Only active bookings can be extended.");

        if (request.NewCheckOut <= booking.CheckIn)
            throw new ArgumentException("New CheckOut must be after current CheckIn.");

        if (request.NewCheckOut <= booking.CheckOut)
            throw new ArgumentException("New CheckOut must be after current CheckOut.");

        var available = await _availabilityService.IsRoomAvailable(booking.RoomId, booking.CheckIn, request.NewCheckOut, id, cancellationToken);
        if (!available)
            throw new RoomNotAvailableException("Room is not available for the extended period.");

        var oldCheckOut = booking.CheckOut;
        booking.CheckOut = request.NewCheckOut;

        var newNightDates = BusinessDateHelper.GetNightDates(oldCheckOut, request.NewCheckOut);
        var existingDates = booking.BookingNights.Select(bn => bn.Date).ToHashSet();
        foreach (var date in newNightDates.Where(d => !existingDates.Contains(d)))
            _context.BookingNights.Add(new BookingNight { BookingId = booking.Id, RoomId = booking.RoomId, Date = date });

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            throw new RoomNotAvailableException("Room is not available for the extended period.");
        }
    }

    public async Task CompleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var booking = await _context.Bookings.FindAsync(new object[] { id }, cancellationToken);

        if (booking is null)
            throw new KeyNotFoundException($"Booking with id {id} not found.");

        if (booking.Status != BookingStatus.Active)
            throw new ArgumentException("Only active bookings can be completed.");

        var payments = await _context.Payments
            .Where(p => p.BookingId == id)
            .ToListAsync(cancellationToken);
        var balance = BookingCalculator.Balance(booking.CheckIn, booking.CheckOut, booking.PricePerNight, payments);
        if (balance > 0)
            throw new ArgumentException("Cannot complete booking with outstanding balance.");

        booking.Status = BookingStatus.Completed;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CancelAsync(int id, CancellationToken cancellationToken = default)
    {
        var booking = await _context.Bookings.FindAsync(new object[] { id }, cancellationToken);

        if (booking is null)
            throw new KeyNotFoundException($"Booking with id {id} not found.");

        if (booking.Status != BookingStatus.Active)
            throw new ArgumentException("Only active bookings can be cancelled.");

        booking.Status = BookingStatus.Cancelled;
        var nights = _context.BookingNights.Where(bn => bn.BookingId == id);
        _context.BookingNights.RemoveRange(nights);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        var msg = ex.InnerException?.Message ?? ex.Message;
        return msg.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("unique", StringComparison.OrdinalIgnoreCase);
    }
}
