using FluentValidation;
using HotelManager.Application.DTOs.Bookings;

namespace HotelManager.Application.Validators;

public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(x => x.RoomId)
            .GreaterThan(0);

        RuleFor(x => x.CheckIn)
            .NotEmpty();

        RuleFor(x => x.CheckOut)
            .NotEmpty()
            .GreaterThan(x => x.CheckIn);

        RuleFor(x => x.PricePerNight)
            .GreaterThan(0);

        RuleFor(x => x.PrimaryGuestId)
            .GreaterThan(0);
    }
}
