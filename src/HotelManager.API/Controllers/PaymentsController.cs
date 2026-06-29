using HotelManager.API.Extensions;
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

    [HttpGet("booking/{bookingId:int}")]
    public async Task<IActionResult> GetByBooking(int bookingId, CancellationToken cancellationToken)
    {
        var payments = await _paymentService.GetByBookingAsync(bookingId, cancellationToken);
        return Ok(payments);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddPaymentRequest request, CancellationToken cancellationToken)
    {
        var payment = await _paymentService.AddAsync(request, User.GetUserId(), cancellationToken);
        return CreatedAtAction(nameof(GetByBooking), new { bookingId = payment.BookingId }, payment);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _paymentService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
