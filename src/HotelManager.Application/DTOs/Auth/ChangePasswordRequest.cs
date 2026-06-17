namespace HotelManager.Application.DTOs.Auth;

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
}
