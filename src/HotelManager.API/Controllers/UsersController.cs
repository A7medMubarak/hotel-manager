using HotelManager.API.Extensions;
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
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await _userService.GetAllAsync(cancellationToken);
        return Ok(users);
    }

    [HttpPost]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var user = await _userService.CreateEmployeeAsync(request, User.GetUserId(), cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { id = user.Id }, user);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _userService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
