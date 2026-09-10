using LibraryManagement.Domain.Entities;

namespace LibraryManagement.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<bool> HasAnyAsync(CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken = default);
}

public interface IMemberRepository
{
    Task<Member?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Member?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Member>> GetPendingAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Member member, CancellationToken cancellationToken = default);
}

public interface IBookRepository
{
    Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Book>> SearchAsync(string? searchTerm, string? category, CancellationToken cancellationToken = default);

    Task AddAsync(Book book, CancellationToken cancellationToken = default);
}

public interface IBookIssueRepository
{
    Task<BookIssue?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<int> CountActiveForMemberAsync(int memberId, CancellationToken cancellationToken = default);

    Task<bool> HasActiveIssueAsync(int bookId, int memberId, CancellationToken cancellationToken = default);

    Task<int> CountActiveForBookAsync(int bookId, CancellationToken cancellationToken = default);

    Task AddAsync(BookIssue issue, CancellationToken cancellationToken = default);
}

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> HasPendingAsync(int bookId, int memberId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Reservation>> GetPendingExpiredAsync(DateTime utcNow, CancellationToken cancellationToken = default);

    Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default);
}
