using HotelManager.Application.DTOs.Payments;
using HotelManager.Application.Services.Interfaces;
using HotelManager.Domain.Entities;
using HotelManager.Domain.Enums;
using HotelManager.Domain.Interfaces;

namespace HotelManager.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IApplicationDbContext _context;

    public PaymentService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PaymentDto>> GetByBookingAsync(int bookingId, CancellationToken cancellationToken = default)
    {
        var bookingExists = await _context.Bookings.AnyAsync(b => b.Id == bookingId, cancellationToken);
        if (!bookingExists)
            throw new KeyNotFoundException($"Booking with id {bookingId} not found.");

        return await _context.Payments
            .Where(p => p.BookingId == bookingId)
            .OrderByDescending(p => p.PaymentDate)
            .Select(p => new PaymentDto
            {
                Id = p.Id,
                BookingId = p.BookingId,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                Notes = p.Notes,
                CreatedByUserId = p.CreatedByUserId,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PaymentDto> AddAsync(AddPaymentRequest request, int createdByUserId, CancellationToken cancellationToken = default)
    {
        var booking = await _context.Bookings.FindAsync(new object[] { request.BookingId }, cancellationToken);
        if (booking is null)
            throw new KeyNotFoundException($"Booking with id {request.BookingId} not found.");

        if (booking.Status != BookingStatus.Active)
            throw new ArgumentException("Payments can only be added to active bookings.");

        if (request.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.");

        var payment = new Payment
        {
            BookingId = request.BookingId,
            Amount = request.Amount,
            PaymentDate = DateTime.UtcNow,
            Notes = request.Notes,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);

        return new PaymentDto
        {
            Id = payment.Id,
            BookingId = payment.BookingId,
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            Notes = payment.Notes,
            CreatedByUserId = payment.CreatedByUserId,
            CreatedAt = payment.CreatedAt
        };
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var payment = await _context.Payments.FindAsync(new object[] { id }, cancellationToken);
        if (payment is null)
            throw new KeyNotFoundException($"Payment with id {id} not found.");

        _context.Payments.Remove(payment);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
