using FluentValidation;
using LibraryManagement.Application.DTOs.Reservations;

namespace LibraryManagement.Application.Validators.Reservations;

public class CreateReservationRequestValidator : AbstractValidator<CreateReservationRequest>
{
    public CreateReservationRequestValidator()
    {
        RuleFor(request => request.BookId).GreaterThan(0);
    }
}
