using FluentValidation;
using LibraryManagement.Application.Common.Exceptions;
using LibraryManagement.Application.DTOs.BookIssues;
using LibraryManagement.Application.Interfaces.Repositories;
using LibraryManagement.Application.Interfaces.Services;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Enums;

namespace LibraryManagement.Application.Services;

public class BookIssueService(
    IBookRepository books,
    IMemberRepository members,
    IUserRepository users,
    IBookIssueRepository issues,
    IUnitOfWork unitOfWork,
    IValidator<IssueBookRequest> issueValidator) : IBookIssueService
{
    public async Task<BookIssueDto> IssueAsync(IssueBookRequest request, CancellationToken cancellationToken = default)
    {
        await issueValidator.ValidateAndThrowAsync(request, cancellationToken);

        var book = await books.GetByIdAsync(request.BookId, cancellationToken)
            ?? throw new NotFoundException("The book was not found.");
        var member = await members.GetByIdAsync(request.MemberId, cancellationToken)
            ?? throw new NotFoundException("The member was not found.");
        var issuingUser = await users.GetByIdAsync(request.IssuedByUserId, cancellationToken)
            ?? throw new NotFoundException("The issuing user was not found.");

        if (!book.IsActive || book.AvailableCopies <= 0)
        {
            throw new BusinessRuleException("The book is not available for issue.");
        }

        if (!member.IsApproved)
        {
            throw new BusinessRuleException("The member is not approved.");
        }

        if (issuingUser.Role is not (UserRole.Admin or UserRole.Librarian))
        {
            throw new BusinessRuleException("Only Admin or Librarian users can issue books.");
        }

        if (await issues.CountActiveForMemberAsync(member.Id, cancellationToken) >= member.MaxBooksAllowed)
        {
            throw new BusinessRuleException("The member has reached the maximum allowed books.");
        }

        if (await issues.HasActiveIssueAsync(book.BookId, member.Id, cancellationToken))
        {
            throw new ConflictException("The member already has this book issued.");
        }

        var now = DateTime.UtcNow;
        var issue = new BookIssue
        {
            BookId = book.BookId,
            MemberId = member.Id,
            IssueDate = now,
            DueDate = now.AddDays(14),
            Status = BookIssueStatus.Issued,
            IssuedByUserId = issuingUser.Id,
            FineAmount = 0,
            FinePaid = false
        };

        book.AvailableCopies--;
        await issues.AddAsync(issue, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(issue);
    }

    public async Task<BookIssueDto> ReturnAsync(ReturnBookRequest request, CancellationToken cancellationToken = default)
    {
        var issue = await issues.GetByIdAsync(request.BookIssueId, cancellationToken)
            ?? throw new NotFoundException("The book issue was not found.");
        var book = await books.GetByIdAsync(issue.BookId, cancellationToken)
            ?? throw new NotFoundException("The book was not found.");
        var returningUser = await users.GetByIdAsync(request.ReturnedToUserId, cancellationToken)
            ?? throw new NotFoundException("The returning user was not found.");

        if (returningUser.Role is not (UserRole.Admin or UserRole.Librarian))
        {
            throw new BusinessRuleException("Only Admin or Librarian users can receive returns.");
        }

        if (issue.Status != BookIssueStatus.Issued)
        {
            throw new BusinessRuleException("Only issued books can be returned.");
        }

        if (request.FineAmount < 0)
        {
            throw new BusinessRuleException("Fine amount cannot be negative.");
        }

        issue.Status = BookIssueStatus.Returned;
        issue.ReturnDate = DateTime.UtcNow;
        issue.ReturnedToUserId = returningUser.Id;
        issue.FineAmount = request.FineAmount;
        issue.FinePaid = request.FineAmount == 0;
        issue.FinePaidDate = null;
        book.AvailableCopies = Math.Min(book.TotalCopies, book.AvailableCopies + 1);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(issue);
    }

    public async Task<BookIssueDto> RenewAsync(RenewBookRequest request, CancellationToken cancellationToken = default)
    {
        var issue = await issues.GetByIdAsync(request.BookIssueId, cancellationToken)
            ?? throw new NotFoundException("The book issue was not found.");

        if (issue.Status != BookIssueStatus.Issued)
        {
            throw new BusinessRuleException("Only issued books can be renewed.");
        }

        if (issue.RenewalCount >= 2)
        {
            throw new BusinessRuleException("The maximum renewal count has been reached.");
        }

        issue.RenewalCount++;
        issue.DueDate = issue.DueDate.AddDays(14);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(issue);
    }

    private static BookIssueDto Map(BookIssue issue) => new()
    {
        BookIssueId = issue.BookIssueId,
        BookId = issue.BookId,
        MemberId = issue.MemberId,
        IssueDate = issue.IssueDate,
        DueDate = issue.DueDate,
        ReturnDate = issue.ReturnDate,
        Status = issue.Status.ToString(),
        RenewalCount = issue.RenewalCount,
        FineAmount = issue.FineAmount,
        FinePaid = issue.FinePaid,
        FinePaidDate = issue.FinePaidDate,
        IssuedByUserId = issue.IssuedByUserId,
        ReturnedToUserId = issue.ReturnedToUserId,
        Remarks = issue.Remarks,
        CreatedAt = issue.CreatedAt,
        UpdatedAt = issue.UpdatedAt
    };
}
