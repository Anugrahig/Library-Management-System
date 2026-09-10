using LibraryManagement.Application.Common.Security;
using LibraryManagement.Application.DTOs.BookIssues;
using LibraryManagement.Application.DTOs.Books;
using LibraryManagement.Application.DTOs.Members;
using LibraryManagement.Application.DTOs.Reservations;
using LibraryManagement.Application.DTOs.Users;
using LibraryManagement.Application.Interfaces.Repositories;
using LibraryManagement.Application.Services;
using LibraryManagement.Application.Validators.BookIssues;
using LibraryManagement.Application.Validators.Books;
using LibraryManagement.Application.Validators.Members;
using LibraryManagement.Application.Validators.Reservations;
using LibraryManagement.Application.Validators.Users;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Enums;
using Xunit;

namespace LibraryManagement.Tests.Application;

public class ApplicationServiceTests
{
    [Fact]
    public async Task Member_service_derives_student_limit_and_starts_pending()
    {
        var userRepository = new FakeUserRepository(new User
        {
            Id = 1,
            Email = "student@example.com",
            PasswordHash = "hash",
            FullName = "Student User",
            Role = UserRole.Student
        });
        var memberRepository = new FakeMemberRepository();
        var service = new MemberService(
            userRepository,
            memberRepository,
            new FakeUnitOfWork(),
            new CreateMemberRequestValidator());

        var result = await service.CreateAsync(1, new CreateMemberRequest
        {
            JoiningDate = new DateTime(2026, 1, 1),
            RegistrationNumber = "202600000001",
            Course = "BTech CSE",
            Semester = "I",
            BatchYear = "2022-26"
        });

        Assert.Equal(MembershipType.Student, result.MembershipType);
        Assert.Equal(5, result.MaxBooksAllowed);
        Assert.False(result.IsApproved);
    }

    [Fact]
    public async Task Member_service_returns_member_for_user()
    {
        var member = new Member
        {
            Id = 4,
            UserId = 1,
            MembershipType = MembershipType.Student,
            MaxBooksAllowed = 5,
            IsApproved = false,
            JoiningDate = new DateTime(2026, 1, 1)
        };
        var service = new MemberService(
            new FakeUserRepository(),
            new FakeMemberRepository(member),
            new FakeUnitOfWork(),
            new CreateMemberRequestValidator());

        var result = await service.GetByUserIdAsync(1);

        Assert.Equal(4, result.Id);
        Assert.False(result.IsApproved);
    }

    [Fact]
    public async Task Member_service_returns_pending_members()
    {
        var pending = new Member
        {
            Id = 4,
            UserId = 1,
            MembershipType = MembershipType.Student,
            MaxBooksAllowed = 5,
            IsApproved = false,
            JoiningDate = new DateTime(2026, 1, 1)
        };
        var service = new MemberService(
            new FakeUserRepository(),
            new FakeMemberRepository(pending),
            new FakeUnitOfWork(),
            new CreateMemberRequestValidator());

        var result = await service.GetPendingAsync();

        var member = Assert.Single(result);
        Assert.Equal(4, member.Id);
    }

    [Fact]
    public async Task User_service_hashes_password_before_persisting()
    {
        var repository = new FakeUserRepository();
        var service = new UserService(
            repository,
            new FakeUnitOfWork(),
            new FakePasswordHasher(),
            new CreateUserRequestValidator());

        await service.CreateAsync(new CreateUserRequest
        {
            Email = "user@example.com",
            Password = "password123",
            FullName = "User Name",
            Role = UserRole.Student
        });

        Assert.Equal("hashed:password123", repository.Added!.PasswordHash);
    }

    [Fact]
    public async Task Book_service_creates_book_with_requested_catalog_values()
    {
        var books = new FakeBookRepository();
        var users = new FakeUserRepository(new User
        {
            Id = 1,
            Email = "librarian@example.com",
            PasswordHash = "hash",
            FullName = "Librarian",
            Role = UserRole.Librarian
        });
        var service = new BookService(books, users, new FakeBookIssueRepository(), new FakeUnitOfWork(), new CreateBookRequestValidator());

        var result = await service.CreateAsync(1, new CreateBookRequest
        {
            Title = "Clean Architecture",
            Category = "Computer Science",
            Author = "Robert Martin",
            TotalCopies = 3
        });

        Assert.Equal("Clean Architecture", result.Title);
        Assert.Equal(3, books.Added!.TotalCopies);
    }

    [Fact]
    public async Task Book_service_rejects_total_copies_below_issued_copies()
    {
        var book = new Book
        {
            BookId = 10,
            Title = "Book",
            Category = "Others",
            TotalCopies = 5,
            AvailableCopies = 2,
            IsActive = true,
            AddedByUserId = 1
        };
        var service = new BookService(
            new FakeBookRepository(book),
            new FakeUserRepository(),
            new FakeBookIssueRepository(3),
            new FakeUnitOfWork(),
            new CreateBookRequestValidator());

        await Assert.ThrowsAsync<LibraryManagement.Application.Common.Exceptions.BusinessRuleException>(() =>
            service.UpdateAsync(10, new UpdateBookRequest
            {
                Title = "Book",
                Category = "Others",
                TotalCopies = 2
            }));
    }

    [Fact]
    public async Task Book_service_deactivates_book_without_deleting_it()
    {
        var book = new Book
        {
            BookId = 10,
            Title = "Book",
            Category = "Others",
            TotalCopies = 1,
            AvailableCopies = 1,
            IsActive = true,
            AddedByUserId = 1
        };
        var service = new BookService(
            new FakeBookRepository(book),
            new FakeUserRepository(),
            new FakeBookIssueRepository(),
            new FakeUnitOfWork(),
            new CreateBookRequestValidator());

        var result = await service.DeactivateAsync(10);

        Assert.False(result.IsActive);
        Assert.Same(book, new[] { book }.Single());
    }

    [Fact]
    public async Task Book_issue_service_decreases_available_copies()
    {
        var book = new Book
        {
            BookId = 10,
            Title = "Book",
            Category = "Others",
            TotalCopies = 2,
            AvailableCopies = 2,
            IsActive = true,
            AddedByUserId = 1
        };
        var member = new Member
        {
            Id = 20,
            UserId = 2,
            MembershipType = MembershipType.Student,
            MaxBooksAllowed = 5,
            IsApproved = true
        };
        var users = new FakeUserRepository(new User
        {
            Id = 1,
            Email = "librarian@example.com",
            PasswordHash = "hash",
            FullName = "Librarian",
            Role = UserRole.Librarian
        });
        var books = new FakeBookRepository(book);
        var members = new FakeMemberRepository(member);
        var issues = new FakeBookIssueRepository();
        var service = new BookIssueService(
            books,
            members,
            users,
            issues,
            new FakeUnitOfWork(),
            new IssueBookRequestValidator());

        var result = await service.IssueAsync(1, new IssueBookRequest
        {
            BookId = 10,
            MemberId = 20
        });

        Assert.Equal(1, book.AvailableCopies);
        Assert.Equal(BookIssueStatus.Issued.ToString(), result.Status);
        Assert.Equal(14, (result.DueDate - result.IssueDate).Days);
    }

    [Fact]
    public async Task Book_issue_service_marks_fine_as_paid_for_member_owner()
    {
        var issue = new BookIssue
        {
            BookIssueId = 30,
            BookId = 10,
            MemberId = 20,
            Status = BookIssueStatus.Returned,
            FineAmount = 25,
            FinePaid = false,
            IssueDate = new DateTime(2026, 1, 1),
            DueDate = new DateTime(2026, 1, 15),
            ReturnDate = new DateTime(2026, 1, 20),
            IssuedByUserId = 1,
            ReturnedToUserId = 1
        };
        var member = new Member
        {
            Id = 20,
            UserId = 2,
            MembershipType = MembershipType.Student,
            MaxBooksAllowed = 5,
            IsApproved = true
        };
        var service = new BookIssueService(
            new FakeBookRepository(),
            new FakeMemberRepository(member),
            new FakeUserRepository(new User
            {
                Id = 2,
                Email = "student@example.com",
                PasswordHash = "hash",
                FullName = "Student",
                Role = UserRole.Student
            }),
            new FakeBookIssueRepository(0, issue),
            new FakeUnitOfWork(),
            new IssueBookRequestValidator());

        var result = await service.PayFineAsync(2, new PayFineRequest { BookIssueId = 30 });

        Assert.True(result.FinePaid);
        Assert.NotNull(result.FinePaidDate);
    }

    [Fact]
    public async Task Reservation_service_creates_seven_day_pending_reservation()
    {
        var book = new Book
        {
            BookId = 10,
            Title = "Book",
            Category = "Others",
            IsActive = true,
            AddedByUserId = 1
        };
        var member = new Member
        {
            Id = 20,
            UserId = 2,
            MembershipType = MembershipType.Student,
            MaxBooksAllowed = 5,
            IsApproved = true
        };
        var reservations = new FakeReservationRepository();
        var service = new ReservationService(
            new FakeBookRepository(book),
            new FakeMemberRepository(member),
            reservations,
            new FakeUnitOfWork(),
            new CreateReservationRequestValidator());

        var result = await service.CreateAsync(new CreateReservationRequest
        {
            BookId = 10,
            MemberId = 20
        });

        Assert.Equal(ReservationStatus.Pending.ToString(), result.Status);
        Assert.Equal(7, (result.ExpiryDate - result.ReservationDate).Days);
    }

    [Fact]
    public async Task Reservation_service_expires_pending_reservations_past_expiry()
    {
        var reservation = new Reservation
        {
            ReservationId = 44,
            BookId = 10,
            MemberId = 20,
            ReservationDate = DateTime.UtcNow.AddDays(-8),
            ExpiryDate = DateTime.UtcNow.AddDays(-1),
            Status = ReservationStatus.Pending
        };
        var repository = new FakeReservationRepository(reservation);
        var service = new ReservationService(
            new FakeBookRepository(),
            new FakeMemberRepository(),
            repository,
            new FakeUnitOfWork(),
            new CreateReservationRequestValidator());

        var expired = await service.ExpirePendingAsync(DateTime.UtcNow);

        Assert.Equal(1, expired);
        Assert.Equal(ReservationStatus.Expired, reservation.Status);
    }
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);

    public Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default) => operation(cancellationToken);
}

internal sealed class FakePasswordHasher : IPasswordHasher
{
    public string Hash(string password) => $"hashed:{password}";

    public bool Verify(string password, string passwordHash) => passwordHash == Hash(password);
}

internal sealed class FakeUserRepository(params User[] users) : IUserRepository
{
    private readonly List<User> _users = users.ToList();

    public User? Added { get; private set; }

    public Task<bool> HasAnyAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_users.Count > 0);

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_users.SingleOrDefault(user => user.Id == id));

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        Task.FromResult(_users.SingleOrDefault(user => user.Email == email));

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        user.Id = _users.Count + 1;
        Added = user;
        _users.Add(user);
        return Task.CompletedTask;
    }
}

internal sealed class FakeMemberRepository(params Member[] members) : IMemberRepository
{
    private readonly List<Member> _members = members.ToList();

    public Task<Member?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_members.SingleOrDefault(member => member.Id == id));

    public Task<Member?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_members.SingleOrDefault(member => member.UserId == userId));

    public Task<IReadOnlyList<Member>> GetPendingAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Member>>(_members.Where(member => !member.IsApproved).ToList());

    public Task AddAsync(Member member, CancellationToken cancellationToken = default)
    {
        member.Id = _members.Count + 1;
        _members.Add(member);
        return Task.CompletedTask;
    }
}

internal sealed class FakeBookRepository(params Book[] books) : IBookRepository
{
    private readonly List<Book> _books = books.ToList();

    public Book? Added { get; private set; }

    public Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_books.SingleOrDefault(book => book.BookId == id));

    public Task<IReadOnlyList<Book>> SearchAsync(string? searchTerm, string? category, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Book>>(_books);

    public Task AddAsync(Book book, CancellationToken cancellationToken = default)
    {
        book.BookId = _books.Count + 1;
        Added = book;
        _books.Add(book);
        return Task.CompletedTask;
    }
}

internal sealed class FakeBookIssueRepository : IBookIssueRepository
{
    private readonly List<BookIssue> _issues = [];

    private readonly int _activeBookCount;

    public FakeBookIssueRepository(int activeBookCount = 0, params BookIssue[] issues)
    {
        _activeBookCount = activeBookCount;
        _issues.AddRange(issues);
    }

    public Task<BookIssue?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_issues.SingleOrDefault(issue => issue.BookIssueId == id));

    public Task<int> CountActiveForMemberAsync(int memberId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_issues.Count(issue => issue.MemberId == memberId && issue.Status == BookIssueStatus.Issued));

    public Task<bool> HasActiveIssueAsync(int bookId, int memberId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_issues.Any(issue => issue.BookId == bookId && issue.MemberId == memberId && issue.Status == BookIssueStatus.Issued));

    public Task<int> CountActiveForBookAsync(int bookId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_activeBookCount + _issues.Count(issue => issue.BookId == bookId && issue.Status == BookIssueStatus.Issued));

    public Task AddAsync(BookIssue issue, CancellationToken cancellationToken = default)
    {
        issue.BookIssueId = _issues.Count + 1;
        _issues.Add(issue);
        return Task.CompletedTask;
    }
}

internal sealed class FakeReservationRepository : IReservationRepository
{
    private readonly List<Reservation> _reservations = [];

    public Task<Reservation?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_reservations.SingleOrDefault(reservation => reservation.ReservationId == id));

    public Task<bool> HasPendingAsync(int bookId, int memberId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_reservations.Any(reservation => reservation.BookId == bookId && reservation.MemberId == memberId && reservation.Status == ReservationStatus.Pending));

    public Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        reservation.ReservationId = _reservations.Count + 1;
        _reservations.Add(reservation);
        return Task.CompletedTask;
    }
}
