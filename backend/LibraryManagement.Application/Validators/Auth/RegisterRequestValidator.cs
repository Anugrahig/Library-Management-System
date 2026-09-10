using FluentValidation;
using LibraryManagement.Application.DTOs.Auth;
using LibraryManagement.Domain.Enums;

namespace LibraryManagement.Application.Validators.Auth;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(request => request.Password)
            .NotEmpty()
            .MinimumLength(8);

        RuleFor(request => request.FullName)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(request => request.MobileNumber)
            .MaximumLength(10);

        RuleFor(request => request.Role)
            .Must(role => role is UserRole.Student or UserRole.Faculty)
            .WithMessage("Public registration is limited to Student and Faculty roles.");
    }
}
