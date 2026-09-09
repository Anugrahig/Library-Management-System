using FluentValidation;
using LibraryManagement.Application.DTOs.BookIssues;

namespace LibraryManagement.Application.Validators.BookIssues;

public class IssueBookRequestValidator : AbstractValidator<IssueBookRequest>
{
    public IssueBookRequestValidator()
    {
        RuleFor(request => request.BookId).GreaterThan(0);
        RuleFor(request => request.MemberId).GreaterThan(0);
        RuleFor(request => request.IssuedByUserId).GreaterThan(0);
    }
}
