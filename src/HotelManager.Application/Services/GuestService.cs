using HotelManager.Application.DTOs.Common;
using HotelManager.Application.DTOs.Guests;
using HotelManager.Application.Services.Interfaces;
using HotelManager.Domain.Entities;
using HotelManager.Domain.Interfaces;

namespace HotelManager.Application.Services;

public class GuestService : IGuestService
{
    private readonly IApplicationDbContext _context;

    public GuestService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<GuestDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var guests = await _context.Guests
            .OrderBy(g => g.FullName)
            .ToListAsync(cancellationToken);

        return guests.Select(MapToDto).ToList();
    }

    public async Task<GuestDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var guest = await _context.Guests
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

        if (guest is null)
            throw new KeyNotFoundException($"Guest with id {id} not found.");

        return MapToDto(guest);
    }

    public async Task<List<GuestDto>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return [];

        query = query.Trim();

        var guests = await _context.Guests
            .Where(g => g.FullName.Contains(query) || g.NationalId.Contains(query))
            .OrderBy(g => g.FullName)
            .ToListAsync(cancellationToken);

        return guests.Select(MapToDto).ToList();
    }

    public async Task<PagedResult<GuestDto>> GetFilteredAsync(GuestFilterRequest filter, CancellationToken cancellationToken = default)
    {
        if (filter.Page < 1) filter.Page = 1;
        if (filter.PageSize < 1) filter.PageSize = 20;

        var query = _context.Guests.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search) && filter.Search.Length >= 2)
        {
            var s = filter.Search.Trim();
            query = query.Where(g => g.FullName.Contains(s) || g.NationalId.Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(filter.Phone))
            query = query.Where(g => g.Phone != null && g.Phone.Contains(filter.Phone));

        if (filter.CreatedFrom.HasValue)
            query = query.Where(g => g.CreatedAt >= filter.CreatedFrom.Value);

        if (filter.CreatedTo.HasValue)
            query = query.Where(g => g.CreatedAt <= filter.CreatedTo.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var guests = await query
            .OrderBy(g => g.FullName)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<GuestDto>
        {
            Items = guests.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<GuestDto> CreateAsync(CreateGuestRequest request, CancellationToken cancellationToken = default)
    {
        var duplicate = await _context.Guests
            .AnyAsync(g => g.NationalId == request.NationalId, cancellationToken);

        if (duplicate)
            throw new ArgumentException($"A guest with National ID {request.NationalId} already exists.");

        var guest = new Guest
        {
            FullName = request.FullName,
            NationalId = request.NationalId,
            Address = request.Address,
            Phone = request.Phone,
            CreatedAt = DateTime.UtcNow
        };

        _context.Guests.Add(guest);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(guest);
    }

    public async Task<GuestDto> UpdateAsync(int id, UpdateGuestRequest request, CancellationToken cancellationToken = default)
    {
        var guest = await _context.Guests.FindAsync(new object[] { id }, cancellationToken);

        if (guest is null)
            throw new KeyNotFoundException($"Guest with id {id} not found.");

        var duplicate = await _context.Guests
            .AnyAsync(g => g.NationalId == request.NationalId && g.Id != id, cancellationToken);

        if (duplicate)
            throw new ArgumentException($"A guest with National ID {request.NationalId} already exists.");

        guest.FullName = request.FullName;
        guest.NationalId = request.NationalId;
        guest.Address = request.Address;
        guest.Phone = request.Phone;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(guest);
    }

    private static GuestDto MapToDto(Guest guest) => new()
    {
        Id = guest.Id,
        FullName = guest.FullName,
        NationalId = guest.NationalId,
        Address = guest.Address,
        Phone = guest.Phone,
        CreatedAt = guest.CreatedAt
    };
}
