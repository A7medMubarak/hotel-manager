using HotelManager.Application.DTOs.Rooms;
using HotelManager.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManager.API.Controllers;

[ApiController]
[Route("api/rooms")]
[Authorize]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] RoomFilterRequest filter, CancellationToken cancellationToken)
    {
        var rooms = await _roomService.GetFilteredAsync(filter, cancellationToken);
        return Ok(rooms);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var room = await _roomService.GetByIdAsync(id, cancellationToken);
        return Ok(room);
    }

    [HttpPost]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Create([FromBody] CreateRoomRequest request, CancellationToken cancellationToken)
    {
        var room = await _roomService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = room.Id }, room);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoomRequest request, CancellationToken cancellationToken)
    {
        var room = await _roomService.UpdateAsync(id, request, cancellationToken);
        return Ok(room);
    }

    [HttpPatch("{id:int}/maintenance")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> ToggleMaintenance(int id, CancellationToken cancellationToken)
    {
        await _roomService.ToggleMaintenanceAsync(id, cancellationToken);
        return NoContent();
    }
}
