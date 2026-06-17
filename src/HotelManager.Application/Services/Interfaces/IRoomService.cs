using HotelManager.Application.DTOs.Common;
using HotelManager.Application.DTOs.Rooms;

namespace HotelManager.Application.Services.Interfaces;

public interface IRoomService
{
    Task<List<RoomDto>> GetAllAsync();
    Task<RoomDto> GetByIdAsync(int id);
    Task<RoomDto> CreateAsync(CreateRoomRequest request);
    Task<RoomDto> UpdateAsync(int id, UpdateRoomRequest request);
    Task ToggleMaintenanceAsync(int id);
    Task<PagedResult<RoomDto>> GetFilteredAsync(RoomFilterRequest filter);
}
