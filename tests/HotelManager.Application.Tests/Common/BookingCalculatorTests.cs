using HotelManager.Application.Common;
using FluentAssertions;

namespace HotelManager.Application.Tests.Common;

public class BookingCalculatorTests
{
    [Fact]
    public void Nights_SameDay_ReturnsZero()
    {
        var checkIn = new DateOnly(2026, 6, 15);
        var checkOut = new DateOnly(2026, 6, 15);
        BookingCalculator.Nights(checkIn, checkOut).Should().Be(0);
    }

    [Fact]
    public void Nights_ThreeDays_ReturnsTwo()
    {
        var checkIn = new DateOnly(2026, 6, 15);
        var checkOut = new DateOnly(2026, 6, 17);
        BookingCalculator.Nights(checkIn, checkOut).Should().Be(2);
    }

    [Fact]
    public void Nights_NextDay_ReturnsOne()
    {
        var checkIn = new DateOnly(2026, 6, 15);
        var checkOut = new DateOnly(2026, 6, 16);
        BookingCalculator.Nights(checkIn, checkOut).Should().Be(1);
    }

    [Fact]
    public void TotalCost_ZeroNights_ReturnsZero()
    {
        var checkIn = new DateOnly(2026, 6, 15);
        var checkOut = new DateOnly(2026, 6, 15);
        BookingCalculator.TotalCost(checkIn, checkOut, 250).Should().Be(0);
    }

    [Fact]
    public void TotalCost_TwoNightsAt250_Returns500()
    {
        var checkIn = new DateOnly(2026, 6, 15);
        var checkOut = new DateOnly(2026, 6, 17);
        BookingCalculator.TotalCost(checkIn, checkOut, 250).Should().Be(500);
    }

    [Fact]
    public void TotalPaid_EmptyPayments_ReturnsZero()
    {
        BookingCalculator.TotalPaid([]).Should().Be(0);
    }

    [Fact]
    public void TotalPaid_MultiplePayments_ReturnsSum()
    {
        var payments = new List<Payment>
        {
            new() { Amount = 100 },
            new() { Amount = 200 },
            new() { Amount = 50 }
        };
        BookingCalculator.TotalPaid(payments).Should().Be(350);
    }

    [Fact]
    public void Balance_FullPayment_ReturnsZero()
    {
        var checkIn = new DateOnly(2026, 6, 15);
        var checkOut = new DateOnly(2026, 6, 17);
        var payments = new List<Payment> { new() { Amount = 500 } };
        BookingCalculator.Balance(checkIn, checkOut, 250, payments).Should().Be(0);
    }

    [Fact]
    public void Balance_PartialPayment_ReturnsRemaining()
    {
        var checkIn = new DateOnly(2026, 6, 15);
        var checkOut = new DateOnly(2026, 6, 17);
        var payments = new List<Payment> { new() { Amount = 200 } };
        BookingCalculator.Balance(checkIn, checkOut, 250, payments).Should().Be(300);
    }

    [Fact]
    public void Balance_NoPayment_ReturnsFullCost()
    {
        var checkIn = new DateOnly(2026, 6, 15);
        var checkOut = new DateOnly(2026, 6, 17);
        BookingCalculator.Balance(checkIn, checkOut, 250, []).Should().Be(500);
    }
}
