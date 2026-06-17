using HotelManager.Domain.Entities;

namespace HotelManager.Application.Services.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}
