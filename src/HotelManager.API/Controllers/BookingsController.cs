using System.Security.Claims;
using HotelManager.Application.DTOs.Bookings;
using HotelManager.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManager.API.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] BookingFilterRequest filter)
    {
        var result = await _bookingService.GetFilteredAsync(filter);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var booking = await _bookingService.GetByIdAsync(id);
        return Ok(booking);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        var result = await _bookingService.SearchAsync(q);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var booking = await _bookingService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
    }

    [HttpPatch("{id}/extend")]
    public async Task<IActionResult> Extend(int id, [FromBody] ExtendBookingRequest request)
    {
        await _bookingService.ExtendAsync(id, request);
        return NoContent();
    }

    [HttpPatch("{id}/complete")]
    public async Task<IActionResult> Complete(int id)
    {
        await _bookingService.CompleteAsync(id);
        return NoContent();
    }

    [HttpPatch("{id}/cancel")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Cancel(int id)
    {
        await _bookingService.CancelAsync(id);
        return NoContent();
    }
}
