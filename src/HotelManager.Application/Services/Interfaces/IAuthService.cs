using HotelManager.Application.DTOs.Auth;

namespace HotelManager.Application.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task ChangePasswordAsync(int userId, ChangePasswordRequest request);
}
