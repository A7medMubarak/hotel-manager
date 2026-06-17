using FluentAssertions;
using HotelManager.Application.DTOs.Guests;
using HotelManager.Application.Validators;

namespace HotelManager.Application.Tests.Validators;

public class CreateGuestRequestValidatorTests
{
    private readonly CreateGuestRequestValidator _validator = new();

    [Fact]
    public void Valid_Request_Passes()
    {
        var request = new CreateGuestRequest
        {
            FullName = "Alice",
            NationalId = "12345678901234",
            Address = "Cairo"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_FullName_Fails()
    {
        var request = new CreateGuestRequest
        {
            FullName = "",
            NationalId = "12345678901234",
            Address = "Cairo"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }
}
