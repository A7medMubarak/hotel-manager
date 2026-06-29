using HotelManager.Application.DTOs.Bookings;
using HotelManager.Application.Services;
using HotelManager.Application.Tests.TestCommon;
using HotelManager.Domain.Entities;
using HotelManager.Domain.Enums;
using FluentAssertions;

namespace HotelManager.Application.Tests.Services;

public class BookingServiceTests
{
    private readonly List<Room> _rooms = new()
    {
        new() { Id = 1, Number = "101", Floor = 1, BedCount = 2, BathroomType = BathroomType.Ensuite, BasePricePerNight = 250 }
    };

    private readonly List<Guest> _guests = new()
    {
        new() { Id = 1, FullName = "Alice", NationalId = "111", Address = "A" },
        new() { Id = 2, FullName = "Bob", NationalId = "222", Address = "B" }
    };

    [Fact]
    public async Task GetActiveAsync_ReturnsActiveOnly()
    {
        var bookings = new List<Booking>
        {
            new() { Id = 1, RoomId = 1, CheckIn = new DateOnly(2026, 6, 15), CheckOut = new DateOnly(2026, 6, 18), PricePerNight = 250, Status = BookingStatus.Active },
            new() { Id = 2, RoomId = 1, CheckIn = new DateOnly(2026, 6, 20), CheckOut = new DateOnly(2026, 6, 22), PricePerNight = 250, Status = BookingStatus.Completed }
        };
        var ctx = MockDbContext.CreateWithData(rooms: _rooms, guests: _guests, bookings: bookings,
            bookingGuests: new List<BookingGuest> { new() { BookingId = 1, GuestId = 1, IsPrimary = true } });
        var bookingQueryService = new BookingQueryService(ctx);
        var bookingAvailabilityService = new BookingAvailabilityService(ctx);
        var service = new BookingService(ctx, bookingQueryService, bookingAvailabilityService);

        var result = await service.GetActiveAsync();

        result.Should().ContainSingle(b => b.Id == 1);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesBooking()
    {
        var ctx = MockDbContext.CreateWithData(rooms: _rooms, guests: _guests);
        var bookingQueryService = new BookingQueryService(ctx);
        var bookingAvailabilityService = new BookingAvailabilityService(ctx);
        var service = new BookingService(ctx, bookingQueryService, bookingAvailabilityService);

        var result = await service.CreateAsync(new CreateBookingRequest
        {
            RoomId = 1,
            CheckIn = new DateOnly(2026, 7, 1),
            CheckOut = new DateOnly(2026, 7, 4),
            PricePerNight = 250,
            PrimaryGuestId = 1,
            AdditionalGuestIds = [2],
            Notes = "Test"
        }, createdByUserId: 1);

        result.RoomNumber.Should().Be("101");
        result.Status.Should().Be("Active");
        result.TotalCost.Should().Be(750);
        result.GuestCount.Should().Be(2);
    }

    [Fact]
    public async Task CreateAsync_OverlappingDates_Throws()
    {
        var bookings = new List<Booking>
        {
            new() { Id = 1, RoomId = 1, CheckIn = new DateOnly(2026, 7, 1), CheckOut = new DateOnly(2026, 7, 5), PricePerNight = 250, Status = BookingStatus.Active }
        };
        var ctx = MockDbContext.CreateWithData(rooms: _rooms, guests: _guests, bookings: bookings,
            bookingGuests: new List<BookingGuest> { new() { BookingId = 1, GuestId = 1, IsPrimary = true } });
        var bookingQueryService = new BookingQueryService(ctx);
        var bookingAvailabilityService = new BookingAvailabilityService(ctx);
        var service = new BookingService(ctx, bookingQueryService, bookingAvailabilityService);

        var act = () => service.CreateAsync(new CreateBookingRequest
        {
            RoomId = 1,
            CheckIn = new DateOnly(2026, 7, 3),
            CheckOut = new DateOnly(2026, 7, 6),
            PricePerNight = 250,
            PrimaryGuestId = 1
        }, createdByUserId: 1);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Room is not available for the selected dates.");
    }

    [Fact]
    public async Task CreateAsync_NonOverlappingDates_Creates()
    {
        var bookings = new List<Booking>
        {
            new() { Id = 1, RoomId = 1, CheckIn = new DateOnly(2026, 7, 1), CheckOut = new DateOnly(2026, 7, 5), PricePerNight = 250, Status = BookingStatus.Active }
        };
        var ctx = MockDbContext.CreateWithData(rooms: _rooms, guests: _guests, bookings: bookings,
            bookingGuests: new List<BookingGuest> { new() { BookingId = 1, GuestId = 1, IsPrimary = true } });
        var bookingQueryService = new BookingQueryService(ctx);
        var bookingAvailabilityService = new BookingAvailabilityService(ctx);
        var service = new BookingService(ctx, bookingQueryService, bookingAvailabilityService);

        var result = await service.CreateAsync(new CreateBookingRequest
        {
            RoomId = 1,
            CheckIn = new DateOnly(2026, 7, 5),
            CheckOut = new DateOnly(2026, 7, 8),
            PricePerNight = 250,
            PrimaryGuestId = 1
        }, createdByUserId: 1);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ExtendAsync_ValidRequest_UpdatesCheckOut()
    {
        var bookings = new List<Booking>
        {
            new() { Id = 1, RoomId = 1, CheckIn = new DateOnly(2026, 7, 1), CheckOut = new DateOnly(2026, 7, 4), PricePerNight = 250, Status = BookingStatus.Active }
        };
        var ctx = MockDbContext.CreateWithData(rooms: _rooms, bookings: bookings,
            bookingGuests: new List<BookingGuest> { new() { BookingId = 1, GuestId = 1, IsPrimary = true } });
        var bookingQueryService = new BookingQueryService(ctx);
        var bookingAvailabilityService = new BookingAvailabilityService(ctx);
        var service = new BookingService(ctx, bookingQueryService, bookingAvailabilityService);

        await service.ExtendAsync(1, new ExtendBookingRequest { NewCheckOut = new DateOnly(2026, 7, 6) });

        var updated = await ctx.Bookings.FindAsync(1);
        updated!.CheckOut.Should().Be(new DateOnly(2026, 7, 6));
    }

    [Fact]
    public async Task ExtendAsync_CompletedBooking_Throws()
    {
        var bookings = new List<Booking>
        {
            new() { Id = 1, RoomId = 1, CheckIn = new DateOnly(2026, 7, 1), CheckOut = new DateOnly(2026, 7, 4), PricePerNight = 250, Status = BookingStatus.Completed }
        };
        var ctx = MockDbContext.CreateWithData(rooms: _rooms, bookings: bookings,
            bookingGuests: new List<BookingGuest> { new() { BookingId = 1, GuestId = 1, IsPrimary = true } });
        var bookingQueryService = new BookingQueryService(ctx);
        var bookingAvailabilityService = new BookingAvailabilityService(ctx);
        var service = new BookingService(ctx, bookingQueryService, bookingAvailabilityService);

        var act = () => service.ExtendAsync(1, new ExtendBookingRequest { NewCheckOut = new DateOnly(2026, 7, 6) });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Only active bookings can be extended.");
    }

    [Fact]
    public async Task CompleteAsync_ActiveBooking_Completes()
    {
        var bookings = new List<Booking>
        {
            new() { Id = 1, RoomId = 1, CheckIn = new DateOnly(2026, 7, 1), CheckOut = new DateOnly(2026, 7, 4), PricePerNight = 250, Status = BookingStatus.Active }
        };
        var ctx = MockDbContext.CreateWithData(rooms: _rooms, bookings: bookings);
        var bookingQueryService = new BookingQueryService(ctx);
        var bookingAvailabilityService = new BookingAvailabilityService(ctx);
        var service = new BookingService(ctx, bookingQueryService, bookingAvailabilityService);

        await service.CompleteAsync(1);

        var updated = await ctx.Bookings.FindAsync(1);
        updated!.Status.Should().Be(BookingStatus.Completed);
    }

    [Fact]
    public async Task CompleteAsync_CancelledBooking_Throws()
    {
        var bookings = new List<Booking>
        {
            new() { Id = 1, RoomId = 1, CheckIn = new DateOnly(2026, 7, 1), CheckOut = new DateOnly(2026, 7, 4), PricePerNight = 250, Status = BookingStatus.Cancelled }
        };
        var ctx = MockDbContext.CreateWithData(rooms: _rooms, bookings: bookings);
        var bookingQueryService = new BookingQueryService(ctx);
        var bookingAvailabilityService = new BookingAvailabilityService(ctx);
        var service = new BookingService(ctx, bookingQueryService, bookingAvailabilityService);

        var act = () => service.CompleteAsync(1);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Only active bookings can be completed.");
    }

    [Fact]
    public async Task CancelAsync_ActiveBooking_Cancels()
    {
        var bookings = new List<Booking>
        {
            new() { Id = 1, RoomId = 1, CheckIn = new DateOnly(2026, 7, 1), CheckOut = new DateOnly(2026, 7, 4), PricePerNight = 250, Status = BookingStatus.Active }
        };
        var ctx = MockDbContext.CreateWithData(rooms: _rooms, bookings: bookings);
        var bookingQueryService = new BookingQueryService(ctx);
        var bookingAvailabilityService = new BookingAvailabilityService(ctx);
        var service = new BookingService(ctx, bookingQueryService, bookingAvailabilityService);

        await service.CancelAsync(1);

        var updated = await ctx.Bookings.FindAsync(1);
        updated!.Status.Should().Be(BookingStatus.Cancelled);
    }

    [Fact]
    public async Task CancelAsync_CompletedBooking_Throws()
    {
        var bookings = new List<Booking>
        {
            new() { Id = 1, RoomId = 1, CheckIn = new DateOnly(2026, 7, 1), CheckOut = new DateOnly(2026, 7, 4), PricePerNight = 250, Status = BookingStatus.Completed }
        };
        var ctx = MockDbContext.CreateWithData(rooms: _rooms, bookings: bookings);
        var bookingQueryService = new BookingQueryService(ctx);
        var bookingAvailabilityService = new BookingAvailabilityService(ctx);
        var service = new BookingService(ctx, bookingQueryService, bookingAvailabilityService);

        var act = () => service.CancelAsync(1);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Only active bookings can be cancelled.");
    }

    [Fact]
    public async Task SearchAsync_ByRoomNumber_ReturnsMatch()
    {
        var bookings = new List<Booking>
        {
            new() { Id = 1, RoomId = 1, CheckIn = new DateOnly(2026, 7, 1), CheckOut = new DateOnly(2026, 7, 4), PricePerNight = 250, Status = BookingStatus.Active }
        };
        var ctx = MockDbContext.CreateWithData(rooms: _rooms, guests: _guests, bookings: bookings,
            bookingGuests: new List<BookingGuest> { new() { BookingId = 1, GuestId = 1, IsPrimary = true } });
        var bookingQueryService = new BookingQueryService(ctx);
        var bookingAvailabilityService = new BookingAvailabilityService(ctx);
        var service = new BookingService(ctx, bookingQueryService, bookingAvailabilityService);

        var result = await service.SearchAsync("101");

        result.Should().ContainSingle(b => b.Id == 1);
    }

    [Fact]
    public async Task SearchAsync_ShortQuery_ReturnsEmpty()
    {
        var bookings = new List<Booking>
        {
            new() { Id = 1, RoomId = 1, CheckIn = new DateOnly(2026, 7, 1), CheckOut = new DateOnly(2026, 7, 4), PricePerNight = 250, Status = BookingStatus.Active }
        };
        var ctx = MockDbContext.CreateWithData(rooms: _rooms, bookings: bookings);
        var bookingQueryService = new BookingQueryService(ctx);
        var bookingAvailabilityService = new BookingAvailabilityService(ctx);
        var service = new BookingService(ctx, bookingQueryService, bookingAvailabilityService);

        var result = await service.SearchAsync("1");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsBookingWithPayments()
    {
        var bookings = new List<Booking>
        {
            new() { Id = 1, RoomId = 1, CheckIn = new DateOnly(2026, 7, 1), CheckOut = new DateOnly(2026, 7, 4), PricePerNight = 250, Status = BookingStatus.Active }
        };
        var payments = new List<Payment>
        {
            new() { Id = 1, BookingId = 1, Amount = 500, PaymentDate = DateTime.UtcNow }
        };
        var ctx = MockDbContext.CreateWithData(rooms: _rooms, guests: _guests, bookings: bookings, payments: payments,
            bookingGuests: new List<BookingGuest> { new() { BookingId = 1, GuestId = 1, IsPrimary = true } });
        var bookingQueryService = new BookingQueryService(ctx);
        var bookingAvailabilityService = new BookingAvailabilityService(ctx);
        var service = new BookingService(ctx, bookingQueryService, bookingAvailabilityService);

        var result = await service.GetByIdAsync(1);

        result.TotalCost.Should().Be(750);
        result.TotalPaid.Should().Be(500);
        result.Balance.Should().Be(250);
    }
}
