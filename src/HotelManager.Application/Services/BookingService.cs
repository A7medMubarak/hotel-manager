using HotelManager.Application.DTOs.Bookings;
using HotelManager.Application.DTOs.Common;
using HotelManager.Application.Services.Interfaces;
using HotelManager.Domain.Entities;
using HotelManager.Domain.Enums;
using HotelManager.Domain.Interfaces;

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

        var roomExists = await _context.Rooms.AnyAsync(r => r.Id == request.RoomId, cancellationToken);
        if (!roomExists)
            throw new ArgumentException($"Room with id {request.RoomId} not found.");

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

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync(cancellationToken);

        return await _queryService.GetByIdAsync(booking.Id, cancellationToken);
    }

    public async Task ExtendAsync(int id, ExtendBookingRequest request, CancellationToken cancellationToken = default)
    {
        var booking = await _context.Bookings.FindAsync(new object[] { id }, cancellationToken);

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
            throw new ArgumentException("Room is not available for the extended period.");

        booking.CheckOut = request.NewCheckOut;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CompleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var booking = await _context.Bookings.FindAsync(new object[] { id }, cancellationToken);

        if (booking is null)
            throw new KeyNotFoundException($"Booking with id {id} not found.");

        if (booking.Status != BookingStatus.Active)
            throw new ArgumentException("Only active bookings can be completed.");

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
        await _context.SaveChangesAsync(cancellationToken);
    }
}
