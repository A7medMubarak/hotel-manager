using FluentAssertions;
using HotelManager.Application.DTOs.Rooms;
using HotelManager.Application.Validators;

namespace HotelManager.Application.Tests.Validators;

public class CreateRoomRequestValidatorTests
{
    private readonly CreateRoomRequestValidator _validator = new();

    [Fact]
    public void Valid_Request_Passes()
    {
        var request = new CreateRoomRequest
        {
            Number = "101",
            Floor = 1,
            BedCount = 2,
            BathroomType = 0,
            BasePricePerNight = 250
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_Number_Fails()
    {
        var request = new CreateRoomRequest
        {
            Number = "",
            Floor = 1,
            BedCount = 2,
            BathroomType = 0,
            BasePricePerNight = 250
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

}
