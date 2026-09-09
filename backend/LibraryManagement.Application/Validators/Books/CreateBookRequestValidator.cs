using FluentValidation;
using LibraryManagement.Application.DTOs.Books;

namespace LibraryManagement.Application.Validators.Books;

public class CreateBookRequestValidator : AbstractValidator<CreateBookRequest>
{
    public static readonly string[] Categories =
    [
        "Computer Science",
        "Information Technology",
        "Electronics & Communication",
        "Mechanical Engineering",
        "Civil Engineering",
        "Mathematics",
        "Physics",
        "Chemistry",
        "Business Administration",
        "English Literature",
        "General Reference",
        "Fiction",
        "Competitive Exam Preparation",
        "Others"
    ];

    public CreateBookRequestValidator()
    {
        RuleFor(request => request.Title)
            .NotEmpty()
            .MaximumLength(250);

        RuleFor(request => request.Category)
            .NotEmpty()
            .MaximumLength(50)
            .Must(category => Categories.Contains(category, StringComparer.Ordinal));

        RuleFor(request => request.ISBN)
            .MaximumLength(20);

        RuleFor(request => request.Author)
            .MaximumLength(200);

        RuleFor(request => request.Edition)
            .MaximumLength(50);

        RuleFor(request => request.CoverImage)
            .MaximumLength(500);

        RuleFor(request => request.ShelfLocation)
            .MaximumLength(100);

        RuleFor(request => request.TotalCopies)
            .GreaterThanOrEqualTo(0);

        RuleFor(request => request.AvailableCopies)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(request => request.TotalCopies);

        RuleFor(request => request.AddedByUserId)
            .GreaterThan(0);
    }
}
