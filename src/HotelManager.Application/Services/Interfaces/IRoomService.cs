using HotelManager.Application.DTOs.Common;
using HotelManager.Application.DTOs.Rooms;

namespace HotelManager.Application.Services.Interfaces;

public interface IRoomService
{
    Task<List<RoomDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RoomDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<RoomDto> CreateAsync(CreateRoomRequest request, CancellationToken cancellationToken = default);
    Task<RoomDto> UpdateAsync(int id, UpdateRoomRequest request, CancellationToken cancellationToken = default);
    Task ToggleMaintenanceAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<RoomDto>> GetFilteredAsync(RoomFilterRequest filter, CancellationToken cancellationToken = default);
}
