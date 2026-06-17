namespace HotelManager.Application.DTOs.Auth;

public class LoginResponse
{
    public string Token { get; set; }
    public string Role { get; set; }
    public DateTime ExpiresAt { get; set; }
}
