using FluentValidation;
using HotelManager.Application.DTOs.Guests;

namespace HotelManager.Application.Validators;

public class UpdateGuestRequestValidator : AbstractValidator<UpdateGuestRequest>
{
    public UpdateGuestRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.NationalId)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.Address)
            .NotEmpty()
            .MaximumLength(300);

        RuleFor(x => x.Phone)
            .MaximumLength(20);
    }
}
