using HotelManager.Application.DTOs.Users;
using HotelManager.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManager.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Owner")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users);
    }

    [HttpPost]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequest request)
    {
        var ownerId = int.Parse(User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

        var user = await _userService.CreateEmployeeAsync(request, ownerId);
        return CreatedAtAction(nameof(GetAll), new { id = user.Id }, user);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _userService.DeleteAsync(id);
        return NoContent();
    }
}
