using System.Security.Claims;
using HotelManager.Application.DTOs.Payments;
using HotelManager.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManager.API.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet("booking/{bookingId}")]
    public async Task<IActionResult> GetByBooking(int bookingId)
    {
        var payments = await _paymentService.GetByBookingAsync(bookingId);
        return Ok(payments);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddPaymentRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var payment = await _paymentService.AddAsync(request, userId);
        return CreatedAtAction(nameof(GetByBooking), new { bookingId = payment.BookingId }, payment);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Delete(int id)
    {
        await _paymentService.DeleteAsync(id);
        return NoContent();
    }
}
