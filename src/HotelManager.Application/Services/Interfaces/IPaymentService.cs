using HotelManager.Application.DTOs.Payments;

namespace HotelManager.Application.Services.Interfaces;

public interface IPaymentService
{
    Task<List<PaymentDto>> GetByBookingAsync(int bookingId);
    Task<PaymentDto> AddAsync(AddPaymentRequest request, int createdByUserId);
    Task DeleteAsync(int id);
}
