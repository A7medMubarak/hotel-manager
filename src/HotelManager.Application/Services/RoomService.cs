using HotelManager.Application.DTOs.Common;
using HotelManager.Application.DTOs.Rooms;
using HotelManager.Application.Services.Interfaces;
using HotelManager.Domain.Entities;
using HotelManager.Domain.Enums;
using HotelManager.Domain.Interfaces;

namespace HotelManager.Application.Services;

public class RoomService : IRoomService
{
    private readonly IApplicationDbContext _context;

    public RoomService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<RoomDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var rooms = await _context.Rooms
            .Select(r => new
            {
                Room = r,
                HasActiveBooking = r.Bookings.Any(b =>
                    b.Status == BookingStatus.Active &&
                    b.CheckIn <= today &&
                    today < b.CheckOut)
            })
            .ToListAsync(cancellationToken);

        return rooms.Select(x => MapToDto(x.Room, x.HasActiveBooking)).ToList();
    }

    public async Task<RoomDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var room = await _context.Rooms
            .Include(r => r.Bookings)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (room is null)
            throw new KeyNotFoundException($"Room with id {id} not found.");

        var hasActiveBooking = room.Bookings.Any(b =>
            b.Status == BookingStatus.Active &&
            b.CheckIn <= today &&
            today < b.CheckOut);

        return MapToDto(room, hasActiveBooking);
    }

    public async Task<RoomDto> CreateAsync(CreateRoomRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Rooms
            .AnyAsync(r => r.Number == request.Number, cancellationToken);

        if (existing)
            throw new ArgumentException($"Room number '{request.Number}' already exists.");

        var room = new Room
        {
            Number = request.Number,
            Floor = request.Floor,
            BedCount = request.BedCount,
            BathroomType = (BathroomType)request.BathroomType,
            BasePricePerNight = request.BasePricePerNight,
            Notes = request.Notes,
            IsUnderMaintenance = false
        };

        _context.Rooms.Add(room);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(room, false);
    }

    public async Task<RoomDto> UpdateAsync(int id, UpdateRoomRequest request, CancellationToken cancellationToken = default)
    {
        var room = await _context.Rooms.FindAsync(new object[] { id }, cancellationToken);

        if (room is null)
            throw new KeyNotFoundException($"Room with id {id} not found.");

        var duplicate = await _context.Rooms
            .AnyAsync(r => r.Number == request.Number && r.Id != id, cancellationToken);

        if (duplicate)
            throw new ArgumentException($"Room number '{request.Number}' is already in use.");

        room.Number = request.Number;
        room.Floor = request.Floor;
        room.BedCount = request.BedCount;
        room.BathroomType = (BathroomType)request.BathroomType;
        room.BasePricePerNight = request.BasePricePerNight;
        room.Notes = request.Notes;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(room, false);
    }

    public async Task<PagedResult<RoomDto>> GetFilteredAsync(RoomFilterRequest filter, CancellationToken cancellationToken = default)
    {
        if (filter.Page < 1) filter.Page = 1;
        if (filter.PageSize < 1) filter.PageSize = 20;

        var today = DateOnly.FromDateTime(DateTime.Today);

        var query = _context.Rooms
            .Select(r => new
            {
                Room = r,
                HasActiveBooking = r.Bookings.Any(b =>
                    b.Status == BookingStatus.Active &&
                    b.CheckIn <= today &&
                    today < b.CheckOut)
            })
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            var statusFilter = filter.Status.ToLower();
            query = query.Where(x =>
                statusFilter == "maintenance" ? x.Room.IsUnderMaintenance :
                statusFilter == "occupied" ? x.HasActiveBooking :
                statusFilter == "available" ? !x.Room.IsUnderMaintenance && !x.HasActiveBooking :
                true);
        }

        if (filter.Floor.HasValue)
            query = query.Where(x => x.Room.Floor == filter.Floor.Value);

        if (filter.MinBedCount.HasValue)
            query = query.Where(x => x.Room.BedCount >= filter.MinBedCount.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var rooms = await query
            .OrderBy(x => x.Room.Number)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<RoomDto>
        {
            Items = rooms.Select(x => MapToDto(x.Room, x.HasActiveBooking)).ToList(),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task ToggleMaintenanceAsync(int id, CancellationToken cancellationToken = default)
    {
        var room = await _context.Rooms.FindAsync(new object[] { id }, cancellationToken);

        if (room is null)
            throw new KeyNotFoundException($"Room with id {id} not found.");

        var today = DateOnly.FromDateTime(DateTime.Today);
        var hasActiveBooking = await _context.Bookings
            .AnyAsync(b => b.RoomId == id && b.Status == BookingStatus.Active &&
                           b.CheckIn <= today && today < b.CheckOut, cancellationToken);

        if (hasActiveBooking)
            throw new ArgumentException("Cannot put room in maintenance while it has active bookings.");

        room.IsUnderMaintenance = !room.IsUnderMaintenance;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static RoomDto MapToDto(Room room, bool hasActiveBooking)
    {
        var status = room.IsUnderMaintenance
            ? "Maintenance"
            : hasActiveBooking
                ? "Occupied"
                : "Available";

        return new RoomDto
        {
            Id = room.Id,
            Number = room.Number,
            Floor = room.Floor,
            BedCount = room.BedCount,
            BathroomType = room.BathroomType == BathroomType.Ensuite ? "Ensuite" : "Shared",
            BasePricePerNight = room.BasePricePerNight,
            Notes = room.Notes,
            IsUnderMaintenance = room.IsUnderMaintenance,
            Status = status
        };
    }
}
