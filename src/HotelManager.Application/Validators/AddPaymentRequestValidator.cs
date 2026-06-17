using FluentValidation;
using HotelManager.Application.DTOs.Payments;

namespace HotelManager.Application.Validators;

public class AddPaymentRequestValidator : AbstractValidator<AddPaymentRequest>
{
    public AddPaymentRequestValidator()
    {
        RuleFor(x => x.BookingId)
            .GreaterThan(0);

        RuleFor(x => x.Amount)
            .GreaterThan(0);
    }
}
