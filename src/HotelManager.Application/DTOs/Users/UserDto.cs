namespace HotelManager.Application.DTOs.Users;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Role { get; set; }
    public DateTime CreatedAt { get; set; }
}
