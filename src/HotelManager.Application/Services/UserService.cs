using HotelManager.Application.DTOs.Users;
using HotelManager.Application.Services.Interfaces;
using HotelManager.Domain.Entities;
using HotelManager.Domain.Enums;
using HotelManager.Domain.Interfaces;

namespace HotelManager.Application.Services;

public class UserService : IUserService
{
    private readonly IApplicationDbContext _context;

    public UserService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _context.Users
            .OrderBy(u => u.Username)
            .ToListAsync(cancellationToken);

        return users.Select(MapToDto).ToList();
    }

    public async Task<UserDto> CreateEmployeeAsync(CreateEmployeeRequest request, int createdByUserId, CancellationToken cancellationToken = default)
    {
        request.Username = request.Username.Trim().ToLowerInvariant();

        var duplicate = await _context.Users
            .AnyAsync(u => u.Username == request.Username, cancellationToken);

        if (duplicate)
            throw new ArgumentException($"Username '{request.Username}' is already taken.");

        var user = new User
        {
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = Enum.Parse<UserRole>(request.Role),
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(user);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id == 1)
            throw new ArgumentException("Cannot delete the primary owner account.");

        var user = await _context.Users.FindAsync(new object[] { id }, cancellationToken);

        if (user is null)
            throw new KeyNotFoundException($"User with id {id} not found.");

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static UserDto MapToDto(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Role = user.Role.ToString(),
        CreatedAt = user.CreatedAt
    };
}
