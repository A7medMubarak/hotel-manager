using HotelManager.Application.DTOs.Rooms;
using HotelManager.Application.Services;
using HotelManager.Application.Tests.TestCommon;
using HotelManager.Domain.Entities;
using HotelManager.Domain.Enums;
using FluentAssertions;

namespace HotelManager.Application.Tests.Services;

public class RoomServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsAllRooms()
    {
        var rooms = new List<Room>
        {
            new() { Id = 1, Number = "101", Floor = 1, BedCount = 2, BathroomType = BathroomType.Ensuite, BasePricePerNight = 250 },
            new() { Id = 2, Number = "102", Floor = 1, BedCount = 1, BathroomType = BathroomType.Shared, BasePricePerNight = 150 }
        };
        var ctx = MockDbContext.CreateWithData(rooms: rooms);
        var service = new RoomService(ctx);

        var result = await service.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingRoom_ReturnsRoom()
    {
        var rooms = new List<Room>
        {
            new() { Id = 1, Number = "101", Floor = 1, BedCount = 2, BathroomType = BathroomType.Ensuite, BasePricePerNight = 250 }
        };
        var ctx = MockDbContext.CreateWithData(rooms: rooms);
        var service = new RoomService(ctx);

        var result = await service.GetByIdAsync(1);

        result.Number.Should().Be("101");
    }

    [Fact]
    public async Task GetByIdAsync_NonExisting_Throws()
    {
        var ctx = MockDbContext.CreateWithData();
        var service = new RoomService(ctx);

        var act = () => service.GetByIdAsync(99);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_DuplicateNumber_Throws()
    {
        var rooms = new List<Room>
        {
            new() { Id = 1, Number = "101", Floor = 1, BedCount = 2, BathroomType = BathroomType.Ensuite, BasePricePerNight = 250 }
        };
        var ctx = MockDbContext.CreateWithData(rooms: rooms);
        var service = new RoomService(ctx);

        var act = () => service.CreateAsync(new CreateRoomRequest
        {
            Number = "101",
            Floor = 2,
            BedCount = 1,
            BathroomType = 1,
            BasePricePerNight = 150
        });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Room number '101' already exists.");
    }

    [Fact]
    public async Task CreateAsync_NewNumber_CreatesRoom()
    {
        var ctx = MockDbContext.CreateWithData();
        var service = new RoomService(ctx);

        var result = await service.CreateAsync(new CreateRoomRequest
        {
            Number = "201",
            Floor = 2,
            BedCount = 2,
            BathroomType = 0,
            BasePricePerNight = 300
        });

        result.Number.Should().Be("201");
        result.Status.Should().Be("Available");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesRoom()
    {
        var rooms = new List<Room>
        {
            new() { Id = 1, Number = "101", Floor = 1, BedCount = 2, BathroomType = BathroomType.Ensuite, BasePricePerNight = 250 }
        };
        var ctx = MockDbContext.CreateWithData(rooms: rooms);
        var service = new RoomService(ctx);

        var result = await service.UpdateAsync(1, new UpdateRoomRequest
        {
            Number = "101A",
            Floor = 1,
            BedCount = 3,
            BathroomType = 0,
            BasePricePerNight = 300,
            Notes = "Updated"
        });

        result.Number.Should().Be("101A");
        result.BedCount.Should().Be(3);
    }

    [Fact]
    public async Task ToggleMaintenanceAsync_ActiveBooking_Throws()
    {
        var rooms = new List<Room>
        {
            new() { Id = 1, Number = "101", Floor = 1, BedCount = 2, BathroomType = BathroomType.Ensuite, BasePricePerNight = 250 }
        };
        var bookings = new List<Booking>
        {
            new() { Id = 1, RoomId = 1, CheckIn = new DateOnly(2026, 6, 15), CheckOut = new DateOnly(2026, 6, 18), Status = BookingStatus.Active }
        };
        var ctx = MockDbContext.CreateWithData(rooms: rooms, bookings: bookings);
        var service = new RoomService(ctx);

        var act = () => service.ToggleMaintenanceAsync(1);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Cannot put room in maintenance while it has active bookings.");
    }

    [Fact]
    public async Task ToggleMaintenanceAsync_NoActiveBooking_Toggles()
    {
        var rooms = new List<Room>
        {
            new() { Id = 1, Number = "101", Floor = 1, BedCount = 2, BathroomType = BathroomType.Ensuite, BasePricePerNight = 250, IsUnderMaintenance = false }
        };
        var ctx = MockDbContext.CreateWithData(rooms: rooms);
        var service = new RoomService(ctx);

        await service.ToggleMaintenanceAsync(1);

        var updated = await ctx.Rooms.FindAsync(1);
        updated!.IsUnderMaintenance.Should().BeTrue();
    }
}
