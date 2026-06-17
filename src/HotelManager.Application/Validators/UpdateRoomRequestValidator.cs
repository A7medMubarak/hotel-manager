using FluentValidation;
using HotelManager.Application.DTOs.Rooms;

namespace HotelManager.Application.Validators;

public class UpdateRoomRequestValidator : AbstractValidator<UpdateRoomRequest>
{
    public UpdateRoomRequestValidator()
    {
        RuleFor(x => x.Number)
            .NotEmpty()
            .MaximumLength(10);

        RuleFor(x => x.Floor)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.BedCount)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.BathroomType)
            .InclusiveBetween(0, 1);

        RuleFor(x => x.BasePricePerNight)
            .GreaterThan(0);
    }
}
