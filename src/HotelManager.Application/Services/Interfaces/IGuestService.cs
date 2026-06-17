using HotelManager.Application.DTOs.Common;
using HotelManager.Application.DTOs.Guests;

namespace HotelManager.Application.Services.Interfaces;

public interface IGuestService
{
    Task<List<GuestDto>> GetAllAsync();
    Task<GuestDto> GetByIdAsync(int id);
    Task<GuestDto> CreateAsync(CreateGuestRequest request);
    Task<GuestDto> UpdateAsync(int id, UpdateGuestRequest request);
    Task<List<GuestDto>> SearchAsync(string query);
    Task<PagedResult<GuestDto>> GetFilteredAsync(GuestFilterRequest filter);
}
