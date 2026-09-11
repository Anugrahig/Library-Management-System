using FluentValidation;
using LibraryManagement.Application.Common.Exceptions;
using LibraryManagement.Application.DTOs.Reservations;
using LibraryManagement.Application.Interfaces.Repositories;
using LibraryManagement.Application.Interfaces.Services;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Enums;

namespace LibraryManagement.Application.Services;

public class ReservationService(
    IBookRepository books,
    IMemberRepository members,
    IUserRepository users,
    IReservationRepository reservations,
    IUnitOfWork unitOfWork,
    IValidator<CreateReservationRequest> validator) : IReservationService
{
    public async Task<ReservationDto> CreateAsync(int userId, CreateReservationRequest request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var book = await books.GetByIdAsync(request.BookId, cancellationToken)
            ?? throw new NotFoundException("The book was not found.");
        var member = await members.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("The member was not found.");

        if (!book.IsActive)
        {
            throw new BusinessRuleException("Inactive books cannot be reserved.");
        }

        if (!member.IsApproved)
        {
            throw new BusinessRuleException("The member is not approved.");
        }

        if (await reservations.HasPendingAsync(book.BookId, member.Id, cancellationToken))
        {
            throw new ConflictException("The member already has a pending reservation for this book.");
        }

        var now = DateTime.UtcNow;
        var reservation = new Reservation
        {
            BookId = book.BookId,
            MemberId = member.Id,
            ReservationDate = now,
            ExpiryDate = now.AddDays(7),
            Status = ReservationStatus.Pending
        };

        await reservations.AddAsync(reservation, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(reservation);
    }

    public Task<ReservationDto> CancelAsync(int userId, ReservationActionRequest request, CancellationToken cancellationToken = default)
        => ChangeStatusAsync(request.ReservationId, ReservationStatus.Cancelled, userId, cancellationToken);

    public Task<ReservationDto> FulfillAsync(ReservationActionRequest request, CancellationToken cancellationToken = default)
        => ChangeStatusAsync(request.ReservationId, ReservationStatus.Fulfilled, null, cancellationToken);

    public async Task<int> ExpirePendingAsync(DateTime utcNow, CancellationToken cancellationToken = default)
    {
        var expiredReservations = await reservations.GetPendingExpiredAsync(utcNow, cancellationToken);
        foreach (var reservation in expiredReservations)
        {
            reservation.Status = ReservationStatus.Expired;
            reservation.UpdatedAt = utcNow;
        }

        if (expiredReservations.Count > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return expiredReservations.Count;
    }

    private async Task<ReservationDto> ChangeStatusAsync(int reservationId, ReservationStatus status, int? userId, CancellationToken cancellationToken)
    {
        var reservation = await reservations.GetByIdAsync(reservationId, cancellationToken)
            ?? throw new NotFoundException("The reservation was not found.");

        if (userId.HasValue)
        {
            var member = await members.GetByIdAsync(reservation.MemberId, cancellationToken)
                ?? throw new NotFoundException("The member was not found.");
            var user = await users.GetByIdAsync(userId.Value, cancellationToken)
                ?? throw new NotFoundException("The user was not found.");

            if (user.Role is not (Domain.Enums.UserRole.Admin or Domain.Enums.UserRole.Librarian) && member.UserId != userId.Value)
            {
                throw new BusinessRuleException("You cannot cancel another member's reservation.");
            }
        }

        if (reservation.Status != ReservationStatus.Pending)
        {
            throw new BusinessRuleException("Only pending reservations can change status.");
        }

        reservation.Status = status;
        if (status == ReservationStatus.Cancelled)
        {
            reservation.CancelledDate = DateTime.UtcNow;
        }
        else
        {
            reservation.FulfilledDate = DateTime.UtcNow;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(reservation);
    }

    private static ReservationDto Map(Reservation reservation) => new()
    {
        ReservationId = reservation.ReservationId,
        BookId = reservation.BookId,
        MemberId = reservation.MemberId,
        ReservationDate = reservation.ReservationDate,
        ExpiryDate = reservation.ExpiryDate,
        FulfilledDate = reservation.FulfilledDate,
        CancelledDate = reservation.CancelledDate,
        Status = reservation.Status.ToString(),
        CreatedAt = reservation.CreatedAt,
        UpdatedAt = reservation.UpdatedAt
    };
}
