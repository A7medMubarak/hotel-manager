using HotelManager.Application.DTOs.Payments;

namespace HotelManager.Application.Services.Interfaces;

public interface IPaymentService
{
    Task<List<PaymentDto>> GetByBookingAsync(int bookingId, CancellationToken cancellationToken = default);
    Task<PaymentDto> AddAsync(AddPaymentRequest request, int createdByUserId, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
