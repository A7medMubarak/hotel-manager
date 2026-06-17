using HotelManager.Application.DTOs.Payments;
using HotelManager.Application.Services;
using HotelManager.Application.Tests.TestCommon;
using HotelManager.Domain.Entities;
using HotelManager.Domain.Enums;
using FluentAssertions;

namespace HotelManager.Application.Tests.Services;

public class PaymentServiceTests
{
    [Fact]
    public async Task GetByBookingAsync_ReturnsPayments()
    {
        var bookings = new List<Booking>
        {
            new() { Id = 1, RoomId = 1, CheckIn = new DateOnly(2026, 7, 1), CheckOut = new DateOnly(2026, 7, 4), PricePerNight = 250, Status = BookingStatus.Active }
        };
        var payments = new List<Payment>
        {
            new() { Id = 1, BookingId = 1, Amount = 200, PaymentDate = DateTime.UtcNow },
            new() { Id = 2, BookingId = 1, Amount = 100, PaymentDate = DateTime.UtcNow.AddHours(-1) }
        };
        var ctx = MockDbContext.CreateWithData(bookings: bookings, payments: payments);
        var service = new PaymentService(ctx);

        var result = await service.GetByBookingAsync(1);

        result.Should().HaveCount(2);
        result.Should().BeInDescendingOrder(p => p.PaymentDate);
    }

    [Fact]
    public async Task GetByBookingAsync_NoPayments_ReturnsEmpty()
    {
        var bookings = new List<Booking>
        {
            new() { Id = 1, RoomId = 1, CheckIn = new DateOnly(2026, 7, 1), CheckOut = new DateOnly(2026, 7, 4), PricePerNight = 250, Status = BookingStatus.Active }
        };
        var ctx = MockDbContext.CreateWithData(bookings: bookings);
        var service = new PaymentService(ctx);

        var result = await service.GetByBookingAsync(1);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByBookingAsync_NonExistingBooking_Throws()
    {
        var ctx = MockDbContext.CreateWithData();
        var service = new PaymentService(ctx);

        var act = () => service.GetByBookingAsync(99);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Booking with id 99 not found.");
    }

    [Fact]
    public async Task AddAsync_ValidPayment_Creates()
    {
        var bookings = new List<Booking>
        {
            new() { Id = 1, RoomId = 1, CheckIn = new DateOnly(2026, 7, 1), CheckOut = new DateOnly(2026, 7, 4), PricePerNight = 250, Status = BookingStatus.Active }
        };
        var ctx = MockDbContext.CreateWithData(bookings: bookings);
        var service = new PaymentService(ctx);

        var result = await service.AddAsync(new AddPaymentRequest { BookingId = 1, Amount = 300, Notes = "Test" }, createdByUserId: 1);

        result.Amount.Should().Be(300);
        result.Notes.Should().Be("Test");
    }

    [Fact]
    public async Task AddAsync_NonActiveBooking_Throws()
    {
        var bookings = new List<Booking>
        {
            new() { Id = 1, RoomId = 1, CheckIn = new DateOnly(2026, 7, 1), CheckOut = new DateOnly(2026, 7, 4), PricePerNight = 250, Status = BookingStatus.Completed }
        };
        var ctx = MockDbContext.CreateWithData(bookings: bookings);
        var service = new PaymentService(ctx);

        var act = () => service.AddAsync(new AddPaymentRequest { BookingId = 1, Amount = 100 }, createdByUserId: 1);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Payments can only be added to active bookings.");
    }

    [Fact]
    public async Task AddAsync_NonPositiveAmount_Throws()
    {
        var bookings = new List<Booking>
        {
            new() { Id = 1, RoomId = 1, CheckIn = new DateOnly(2026, 7, 1), CheckOut = new DateOnly(2026, 7, 4), PricePerNight = 250, Status = BookingStatus.Active }
        };
        var ctx = MockDbContext.CreateWithData(bookings: bookings);
        var service = new PaymentService(ctx);

        var act = () => service.AddAsync(new AddPaymentRequest { BookingId = 1, Amount = 0 }, createdByUserId: 1);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Amount must be greater than zero.");
    }

    [Fact]
    public async Task AddAsync_NegativeAmount_Throws()
    {
        var bookings = new List<Booking>
        {
            new() { Id = 1, RoomId = 1, CheckIn = new DateOnly(2026, 7, 1), CheckOut = new DateOnly(2026, 7, 4), PricePerNight = 250, Status = BookingStatus.Active }
        };
        var ctx = MockDbContext.CreateWithData(bookings: bookings);
        var service = new PaymentService(ctx);

        var act = () => service.AddAsync(new AddPaymentRequest { BookingId = 1, Amount = -50 }, createdByUserId: 1);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Amount must be greater than zero.");
    }

    [Fact]
    public async Task DeleteAsync_ExistingPayment_Removes()
    {
        var payments = new List<Payment>
        {
            new() { Id = 1, BookingId = 1, Amount = 100, PaymentDate = DateTime.UtcNow }
        };
        var ctx = MockDbContext.CreateWithData(payments: payments);
        var service = new PaymentService(ctx);

        await service.DeleteAsync(1);

        var remaining = await ctx.Payments.ToListAsync();
        remaining.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_NonExisting_Throws()
    {
        var ctx = MockDbContext.CreateWithData();
        var service = new PaymentService(ctx);

        var act = () => service.DeleteAsync(99);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Payment with id 99 not found.");
    }
}
