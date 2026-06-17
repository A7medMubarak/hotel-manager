using HotelManager.Application.DTOs.Guests;
using HotelManager.Application.Services;
using HotelManager.Application.Tests.TestCommon;
using HotelManager.Domain.Entities;
using FluentAssertions;

namespace HotelManager.Application.Tests.Services;

public class GuestServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsAllGuests()
    {
        var guests = new List<Guest>
        {
            new() { Id = 1, FullName = "Alice", NationalId = "111", Address = "Addr1" },
            new() { Id = 2, FullName = "Bob", NationalId = "222", Address = "Addr2" }
        };
        var ctx = MockDbContext.CreateWithData(guests: guests);
        var service = new GuestService(ctx);

        var result = await service.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_Existing_ReturnsGuest()
    {
        var guests = new List<Guest>
        {
            new() { Id = 1, FullName = "Alice", NationalId = "111", Address = "Addr1" }
        };
        var ctx = MockDbContext.CreateWithData(guests: guests);
        var service = new GuestService(ctx);

        var result = await service.GetByIdAsync(1);

        result.FullName.Should().Be("Alice");
    }

    [Fact]
    public async Task GetByIdAsync_NonExisting_Throws()
    {
        var ctx = MockDbContext.CreateWithData();
        var service = new GuestService(ctx);

        var act = () => service.GetByIdAsync(99);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_DuplicateNationalId_Throws()
    {
        var guests = new List<Guest>
        {
            new() { Id = 1, FullName = "Alice", NationalId = "111", Address = "Addr1" }
        };
        var ctx = MockDbContext.CreateWithData(guests: guests);
        var service = new GuestService(ctx);

        var act = () => service.CreateAsync(new CreateGuestRequest
        {
            FullName = "Alice 2",
            NationalId = "111",
            Address = "Addr2"
        });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("A guest with National ID 111 already exists.");
    }

    [Fact]
    public async Task CreateAsync_NewNationalId_CreatesGuest()
    {
        var ctx = MockDbContext.CreateWithData();
        var service = new GuestService(ctx);

        var result = await service.CreateAsync(new CreateGuestRequest
        {
            FullName = "Charlie",
            NationalId = "333",
            Address = "Addr3"
        });

        result.FullName.Should().Be("Charlie");
    }

    [Fact]
    public async Task SearchAsync_ByName_ReturnsMatches()
    {
        var guests = new List<Guest>
        {
            new() { Id = 1, FullName = "Alice Wonderland", NationalId = "111", Address = "A" },
            new() { Id = 2, FullName = "Bob", NationalId = "222", Address = "B" }
        };
        var ctx = MockDbContext.CreateWithData(guests: guests);
        var service = new GuestService(ctx);

        var result = await service.SearchAsync("Alice");

        result.Should().ContainSingle(g => g.FullName == "Alice Wonderland");
    }

    [Fact]
    public async Task SearchAsync_ByNationalId_ReturnsMatches()
    {
        var guests = new List<Guest>
        {
            new() { Id = 1, FullName = "Alice", NationalId = "111", Address = "A" },
            new() { Id = 2, FullName = "Bob", NationalId = "222", Address = "B" }
        };
        var ctx = MockDbContext.CreateWithData(guests: guests);
        var service = new GuestService(ctx);

        var result = await service.SearchAsync("222");

        result.Should().ContainSingle(g => g.FullName == "Bob");
    }

    [Fact]
    public async Task SearchAsync_ShortQuery_ReturnsEmpty()
    {
        var guests = new List<Guest>
        {
            new() { Id = 1, FullName = "Alice", NationalId = "111", Address = "A" }
        };
        var ctx = MockDbContext.CreateWithData(guests: guests);
        var service = new GuestService(ctx);

        var result = await service.SearchAsync("A");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_UpdatesGuest()
    {
        var guests = new List<Guest>
        {
            new() { Id = 1, FullName = "Alice", NationalId = "111", Address = "Old", Phone = null }
        };
        var ctx = MockDbContext.CreateWithData(guests: guests);
        var service = new GuestService(ctx);

        var result = await service.UpdateAsync(1, new UpdateGuestRequest
        {
            FullName = "Alice Updated",
            NationalId = "111",
            Address = "New",
            Phone = "01000000000"
        });

        result.FullName.Should().Be("Alice Updated");
        result.Phone.Should().Be("01000000000");
    }

    [Fact]
    public async Task UpdateAsync_DuplicateNationalId_Throws()
    {
        var guests = new List<Guest>
        {
            new() { Id = 1, FullName = "Alice", NationalId = "111", Address = "A" },
            new() { Id = 2, FullName = "Bob", NationalId = "222", Address = "B" }
        };
        var ctx = MockDbContext.CreateWithData(guests: guests);
        var service = new GuestService(ctx);

        var act = () => service.UpdateAsync(1, new UpdateGuestRequest
        {
            FullName = "Alice",
            NationalId = "222",
            Address = "A"
        });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("A guest with National ID 222 already exists.");
    }
}
