using FluentValidation;
using HotelManager.Application.DTOs.Bookings;

namespace HotelManager.Application.Validators;

public class ExtendBookingRequestValidator : AbstractValidator<ExtendBookingRequest>
{
    public ExtendBookingRequestValidator()
    {
        RuleFor(x => x.NewCheckOut)
            .NotEmpty();
    }
}
