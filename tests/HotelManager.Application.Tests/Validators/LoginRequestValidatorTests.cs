using FluentAssertions;
using HotelManager.Application.DTOs.Auth;
using HotelManager.Application.Validators;

namespace HotelManager.Application.Tests.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void Valid_Request_Passes()
    {
        var request = new LoginRequest
        {
            Username = "admin",
            Password = "Admin123!"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_Username_Fails()
    {
        var request = new LoginRequest
        {
            Username = "",
            Password = "Admin123!"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_Password_Fails()
    {
        var request = new LoginRequest
        {
            Username = "admin",
            Password = ""
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Both_Empty_Fails()
    {
        var request = new LoginRequest
        {
            Username = "",
            Password = ""
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
    }
}
