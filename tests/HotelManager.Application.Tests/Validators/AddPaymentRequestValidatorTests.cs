using FluentAssertions;
using HotelManager.Application.DTOs.Payments;
using HotelManager.Application.Validators;

namespace HotelManager.Application.Tests.Validators;

public class AddPaymentRequestValidatorTests
{
    private readonly AddPaymentRequestValidator _validator = new();

    [Fact]
    public void Valid_Request_Passes()
    {
        var request = new AddPaymentRequest
        {
            BookingId = 1,
            Amount = 100
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Zero_BookingId_Fails()
    {
        var request = new AddPaymentRequest
        {
            BookingId = 0,
            Amount = 100
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Zero_Amount_Fails()
    {
        var request = new AddPaymentRequest
        {
            BookingId = 1,
            Amount = 0
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }
}
