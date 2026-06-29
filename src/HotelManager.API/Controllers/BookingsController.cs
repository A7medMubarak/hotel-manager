using HotelManager.API.Extensions;
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
    public async Task<IActionResult> GetAll([FromQuery] BookingFilterRequest filter, CancellationToken cancellationToken)
    {
        var result = await _bookingService.GetFilteredAsync(filter, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var booking = await _bookingService.GetByIdAsync(id, cancellationToken);
        return Ok(booking);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q, CancellationToken cancellationToken)
    {
        var result = await _bookingService.SearchAsync(q, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var booking = await _bookingService.CreateAsync(request, User.GetUserId(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
    }

    [HttpPatch("{id:int}/extend")]
    public async Task<IActionResult> Extend(int id, [FromBody] ExtendBookingRequest request, CancellationToken cancellationToken)
    {
        await _bookingService.ExtendAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:int}/complete")]
    public async Task<IActionResult> Complete(int id, CancellationToken cancellationToken)
    {
        await _bookingService.CompleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:int}/cancel")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        await _bookingService.CancelAsync(id, cancellationToken);
        return NoContent();
    }
}
