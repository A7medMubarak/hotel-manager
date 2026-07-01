namespace HotelManager.Application.DTOs.Users;

public class CreateEmployeeRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Role { get; set; } = "Employee";
}
