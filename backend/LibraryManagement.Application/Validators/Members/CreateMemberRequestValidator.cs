using FluentValidation;
using LibraryManagement.Application.DTOs.Members;

namespace LibraryManagement.Application.Validators.Members;

public class CreateMemberRequestValidator : AbstractValidator<CreateMemberRequest>
{
    public CreateMemberRequestValidator()
    {
        RuleFor(request => request.UserId)
            .GreaterThan(0);

        RuleFor(request => request.JoiningDate)
            .NotEmpty();

        RuleFor(request => request.RegistrationNumber)
            .Length(12, 14)
            .When(request => request.RegistrationNumber is not null);

        RuleFor(request => request.EmployeeId)
            .Length(6, 8)
            .When(request => request.EmployeeId is not null);

        RuleFor(request => request.Course)
            .MaximumLength(30);

        RuleFor(request => request.Semester)
            .MaximumLength(10);

        RuleFor(request => request.BatchYear)
            .MaximumLength(7);

        RuleFor(request => request.Designation)
            .MaximumLength(30);

        RuleFor(request => request.Department)
            .MaximumLength(10);
    }
}
