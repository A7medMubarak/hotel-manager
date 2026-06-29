using HotelManager.Application.DTOs.Common;
using HotelManager.Application.DTOs.Guests;

namespace HotelManager.Application.Services.Interfaces;

public interface IGuestService
{
    Task<List<GuestDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<GuestDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<GuestDto> CreateAsync(CreateGuestRequest request, CancellationToken cancellationToken = default);
    Task<GuestDto> UpdateAsync(int id, UpdateGuestRequest request, CancellationToken cancellationToken = default);
    Task<List<GuestDto>> SearchAsync(string query, CancellationToken cancellationToken = default);
    Task<PagedResult<GuestDto>> GetFilteredAsync(GuestFilterRequest filter, CancellationToken cancellationToken = default);
}
