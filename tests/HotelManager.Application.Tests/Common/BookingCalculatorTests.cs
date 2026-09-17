using HotelManager.Application.Common;
using FluentAssertions;

namespace HotelManager.Application.Tests.Common;

public class BookingCalculatorTests
{
    [Fact]
    public void Nights_SameDay_ReturnsOne()
    {
        var checkIn = new DateTime(2026, 6, 15, 5, 0, 0, DateTimeKind.Utc);
        var checkOut = new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        BookingCalculator.Nights(checkIn, checkOut).Should().Be(1);
    }

    [Fact]
    public void Nights_ThreeDays_ReturnsTwo()
    {
        var checkIn = new DateTime(2026, 6, 15, 14, 0, 0, DateTimeKind.Utc);
        var checkOut = new DateTime(2026, 6, 17, 12, 0, 0, DateTimeKind.Utc);
        BookingCalculator.Nights(checkIn, checkOut).Should().Be(2);
    }

    [Fact]
    public void Nights_NextDay_ReturnsOne()
    {
        var checkIn = new DateTime(2026, 6, 15, 14, 0, 0, DateTimeKind.Utc);
        var checkOut = new DateTime(2026, 6, 16, 12, 0, 0, DateTimeKind.Utc);
        BookingCalculator.Nights(checkIn, checkOut).Should().Be(1);
    }

    [Fact]
    public void TotalCost_SameDay_ReturnsOneNight()
    {
        var checkIn = new DateTime(2026, 6, 15, 5, 0, 0, DateTimeKind.Utc);
        var checkOut = new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        BookingCalculator.TotalCost(checkIn, checkOut, 250).Should().Be(250);
    }

    [Fact]
    public void TotalCost_TwoNightsAt250_Returns500()
    {
        var checkIn = new DateTime(2026, 6, 15, 14, 0, 0, DateTimeKind.Utc);
        var checkOut = new DateTime(2026, 6, 17, 12, 0, 0, DateTimeKind.Utc);
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
        var checkIn = new DateTime(2026, 6, 15, 14, 0, 0, DateTimeKind.Utc);
        var checkOut = new DateTime(2026, 6, 17, 12, 0, 0, DateTimeKind.Utc);
        var payments = new List<Payment> { new() { Amount = 500 } };
        BookingCalculator.Balance(checkIn, checkOut, 250, payments).Should().Be(0);
    }

    [Fact]
    public void Balance_PartialPayment_ReturnsRemaining()
    {
        var checkIn = new DateTime(2026, 6, 15, 14, 0, 0, DateTimeKind.Utc);
        var checkOut = new DateTime(2026, 6, 17, 12, 0, 0, DateTimeKind.Utc);
        var payments = new List<Payment> { new() { Amount = 200 } };
        BookingCalculator.Balance(checkIn, checkOut, 250, payments).Should().Be(300);
    }

    [Fact]
    public void Balance_NoPayment_ReturnsFullCost()
    {
        var checkIn = new DateTime(2026, 6, 15, 14, 0, 0, DateTimeKind.Utc);
        var checkOut = new DateTime(2026, 6, 17, 12, 0, 0, DateTimeKind.Utc);
        BookingCalculator.Balance(checkIn, checkOut, 250, []).Should().Be(500);
    }

    [Fact]
    public void GetNightDates_SameDay_ReturnsOneDate()
    {
        var checkIn = new DateTime(2026, 6, 15, 5, 0, 0, DateTimeKind.Utc);
        var checkOut = new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        var dates = BusinessDateHelper.GetNightDates(checkIn, checkOut);
        dates.Should().ContainSingle().Which.Should().Be(new DateOnly(2026, 6, 15));
    }

    [Fact]
    public void GetNightDates_ThreeNights_ReturnsCorrectDates()
    {
        var checkIn = new DateTime(2026, 6, 15, 14, 0, 0, DateTimeKind.Utc);
        var checkOut = new DateTime(2026, 6, 18, 12, 0, 0, DateTimeKind.Utc);
        var dates = BusinessDateHelper.GetNightDates(checkIn, checkOut);
        dates.Should().HaveCount(3);
        dates.Should().ContainInOrder(new DateOnly(2026, 6, 15), new DateOnly(2026, 6, 16), new DateOnly(2026, 6, 17));
    }
}
