using FluentAssertions;
using HotelManager.Application.DTOs.Bookings;
using HotelManager.Application.Validators;

namespace HotelManager.Application.Tests.Validators;

public class CreateBookingRequestValidatorTests
{
    private readonly CreateBookingRequestValidator _validator = new();

    [Fact]
    public void Valid_Request_Passes()
    {
        var request = new CreateBookingRequest
        {
            RoomId = 1,
            CheckIn = new DateTime(2026, 7, 1, 14, 0, 0, DateTimeKind.Utc),
            CheckOut = new DateTime(2026, 7, 4, 12, 0, 0, DateTimeKind.Utc),
            PricePerNight = 250,
            PrimaryGuestId = 1
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Valid_SameDay_Passes()
    {
        var request = new CreateBookingRequest
        {
            RoomId = 1,
            CheckIn = new DateTime(2026, 7, 1, 5, 0, 0, DateTimeKind.Utc),
            CheckOut = new DateTime(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc),
            PricePerNight = 250,
            PrimaryGuestId = 1
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Invalid_RoomId_Fails()
    {
        var request = new CreateBookingRequest
        {
            RoomId = 0,
            CheckIn = new DateTime(2026, 7, 1, 14, 0, 0, DateTimeKind.Utc),
            CheckOut = new DateTime(2026, 7, 4, 12, 0, 0, DateTimeKind.Utc),
            PricePerNight = 250,
            PrimaryGuestId = 1
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CheckOut_Before_CheckIn_Fails()
    {
        var request = new CreateBookingRequest
        {
            RoomId = 1,
            CheckIn = new DateTime(2026, 7, 4, 14, 0, 0, DateTimeKind.Utc),
            CheckOut = new DateTime(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc),
            PricePerNight = 250,
            PrimaryGuestId = 1
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Zero_PricePerNight_Fails()
    {
        var request = new CreateBookingRequest
        {
            RoomId = 1,
            CheckIn = new DateTime(2026, 7, 1, 14, 0, 0, DateTimeKind.Utc),
            CheckOut = new DateTime(2026, 7, 4, 12, 0, 0, DateTimeKind.Utc),
            PricePerNight = 0,
            PrimaryGuestId = 1
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }
}
