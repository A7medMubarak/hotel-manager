using HotelManager.Application.DTOs.Users;

namespace HotelManager.Application.Services.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserDto> CreateEmployeeAsync(CreateEmployeeRequest request, int createdByUserId, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
