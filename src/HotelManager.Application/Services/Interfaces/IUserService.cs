using HotelManager.Application.DTOs.Users;

namespace HotelManager.Application.Services.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetAllAsync();
    Task<UserDto> CreateEmployeeAsync(CreateEmployeeRequest request, int createdByUserId);
    Task DeleteAsync(int id);
}
