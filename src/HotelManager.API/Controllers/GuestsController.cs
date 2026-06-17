using HotelManager.Application.DTOs.Guests;
using HotelManager.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManager.API.Controllers;

[ApiController]
[Route("api/guests")]
[Authorize]
public class GuestsController : ControllerBase
{
    private readonly IGuestService _guestService;

    public GuestsController(IGuestService guestService)
    {
        _guestService = guestService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GuestFilterRequest filter)
    {
        var guests = await _guestService.GetFilteredAsync(filter);
        return Ok(guests);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var guest = await _guestService.GetByIdAsync(id);
        return Ok(guest);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        var guests = await _guestService.SearchAsync(q);
        return Ok(guests);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGuestRequest request)
    {
        var guest = await _guestService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = guest.Id }, guest);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateGuestRequest request)
    {
        var guest = await _guestService.UpdateAsync(id, request);
        return Ok(guest);
    }
}
