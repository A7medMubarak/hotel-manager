using FluentValidation;
using HotelManager.Application.DTOs.Users;

namespace HotelManager.Application.Validators;

public class CreateEmployeeRequestValidator : AbstractValidator<CreateEmployeeRequest>
{
    public CreateEmployeeRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a number.");

        RuleFor(x => x.Role)
            .Must(r => r == "Employee" || r == "Owner")
            .WithMessage("Role must be 'Employee' or 'Owner'.");
    }
}
