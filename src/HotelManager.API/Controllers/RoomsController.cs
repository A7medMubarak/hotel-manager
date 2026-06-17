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
    public async Task<IActionResult> GetAll([FromQuery] RoomFilterRequest filter)
    {
        var rooms = await _roomService.GetFilteredAsync(filter);
        return Ok(rooms);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var room = await _roomService.GetByIdAsync(id);
        return Ok(room);
    }

    [HttpPost]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Create([FromBody] CreateRoomRequest request)
    {
        var room = await _roomService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = room.Id }, room);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoomRequest request)
    {
        var room = await _roomService.UpdateAsync(id, request);
        return Ok(room);
    }

    [HttpPatch("{id}/maintenance")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> ToggleMaintenance(int id)
    {
        await _roomService.ToggleMaintenanceAsync(id);
        return NoContent();
    }
}
