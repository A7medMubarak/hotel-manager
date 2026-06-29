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
    public async Task<IActionResult> GetAll([FromQuery] GuestFilterRequest filter, CancellationToken cancellationToken)
    {
        var guests = await _guestService.GetFilteredAsync(filter, cancellationToken);
        return Ok(guests);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var guest = await _guestService.GetByIdAsync(id, cancellationToken);
        return Ok(guest);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q, CancellationToken cancellationToken)
    {
        var guests = await _guestService.SearchAsync(q, cancellationToken);
        return Ok(guests);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGuestRequest request, CancellationToken cancellationToken)
    {
        var guest = await _guestService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = guest.Id }, guest);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateGuestRequest request, CancellationToken cancellationToken)
    {
        var guest = await _guestService.UpdateAsync(id, request, cancellationToken);
        return Ok(guest);
    }
}
