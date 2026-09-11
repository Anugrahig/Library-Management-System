using LibraryManagement.Application.Interfaces.Repositories;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Enums;
using LibraryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Infrastructure.Repositories;

public sealed class EfUserRepository(LibraryDbContext context) : IUserRepository
{
    public Task<bool> HasAnyAsync(CancellationToken cancellationToken = default) =>
        context.Users.AnyAsync(cancellationToken);

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.Users.SingleOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        context.Users.SingleOrDefaultAsync(user => user.Email == email, cancellationToken);

    public async Task<IReadOnlyList<User>> SearchAsync(
        UserRole? role,
        bool? isActive,
        string? searchTerm,
        CancellationToken cancellationToken = default)
    {
        var query = context.Users.AsNoTracking().AsQueryable();

        if (role.HasValue)
        {
            query = query.Where(user => user.Role == role.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(user => user.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = $"%{searchTerm.Trim()}%";
            query = query.Where(user =>
                EF.Functions.Like(user.Email, term) ||
                EF.Functions.Like(user.FullName, term));
        }

        return await query.OrderBy(user => user.FullName).ToListAsync(cancellationToken);
    }

    public Task<int> CountActiveAdminsAsync(CancellationToken cancellationToken = default) =>
        context.Users.CountAsync(user => user.Role == UserRole.Admin && user.IsActive, cancellationToken);

    public Task AddAsync(User user, CancellationToken cancellationToken = default) =>
        context.Users.AddAsync(user, cancellationToken).AsTask();
}

public sealed class EfMemberRepository(LibraryDbContext context) : IMemberRepository
{
    public Task<Member?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.Members.SingleOrDefaultAsync(member => member.Id == id, cancellationToken);

    public Task<Member?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default) =>
        context.Members.SingleOrDefaultAsync(member => member.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<Member>> GetPendingAsync(CancellationToken cancellationToken = default) =>
        await context.Members
            .AsNoTracking()
            .Where(member => !member.IsApproved)
            .OrderBy(member => member.JoiningDate)
            .ToListAsync(cancellationToken);

    public Task AddAsync(Member member, CancellationToken cancellationToken = default) =>
        context.Members.AddAsync(member, cancellationToken).AsTask();
}

public sealed class EfBookRepository(LibraryDbContext context) : IBookRepository
{
    public Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.Books.SingleOrDefaultAsync(book => book.BookId == id, cancellationToken);

    public async Task<IReadOnlyList<Book>> SearchAsync(
        string? searchTerm,
        string? category,
        CancellationToken cancellationToken = default)
    {
        var query = context.Books.AsNoTracking().Where(book => book.IsActive);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = $"%{searchTerm.Trim()}%";
            query = query.Where(book =>
                EF.Functions.Like(book.Title, term) ||
                (book.Author != null && EF.Functions.Like(book.Author, term)));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(book => book.Category == category);
        }

        return await query.OrderBy(book => book.Title).ToListAsync(cancellationToken);
    }

    public Task AddAsync(Book book, CancellationToken cancellationToken = default) =>
        context.Books.AddAsync(book, cancellationToken).AsTask();
}

public sealed class EfBookIssueRepository(LibraryDbContext context) : IBookIssueRepository
{
    public Task<BookIssue?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.BookIssues.SingleOrDefaultAsync(issue => issue.BookIssueId == id, cancellationToken);

    public Task<int> CountActiveForMemberAsync(int memberId, CancellationToken cancellationToken = default) =>
        context.BookIssues.CountAsync(
            issue => issue.MemberId == memberId && issue.Status == BookIssueStatus.Issued,
            cancellationToken);

    public Task<bool> HasActiveIssueAsync(int bookId, int memberId, CancellationToken cancellationToken = default) =>
        context.BookIssues.AnyAsync(
            issue => issue.BookId == bookId && issue.MemberId == memberId && issue.Status == BookIssueStatus.Issued,
            cancellationToken);

    public Task<int> CountActiveForBookAsync(int bookId, CancellationToken cancellationToken = default) =>
        context.BookIssues.CountAsync(
            issue => issue.BookId == bookId && issue.Status == BookIssueStatus.Issued,
            cancellationToken);

    public Task AddAsync(BookIssue issue, CancellationToken cancellationToken = default) =>
        context.BookIssues.AddAsync(issue, cancellationToken).AsTask();
}

public sealed class EfReservationRepository(LibraryDbContext context) : IReservationRepository
{
    public Task<Reservation?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.Reservations.SingleOrDefaultAsync(reservation => reservation.ReservationId == id, cancellationToken);

    public Task<bool> HasPendingAsync(int bookId, int memberId, CancellationToken cancellationToken = default) =>
        context.Reservations.AnyAsync(
            reservation => reservation.BookId == bookId &&
                           reservation.MemberId == memberId &&
            reservation.Status == ReservationStatus.Pending,
            cancellationToken);

    public async Task<IReadOnlyList<Reservation>> GetPendingExpiredAsync(
        DateTime utcNow,
        CancellationToken cancellationToken = default) =>
        await context.Reservations
            .Where(reservation => reservation.Status == ReservationStatus.Pending && reservation.ExpiryDate <= utcNow)
            .ToListAsync(cancellationToken);

    public Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default) =>
        context.Reservations.AddAsync(reservation, cancellationToken).AsTask();
}
