using LibraryManagement.Application.DTOs.BookIssues;
using LibraryManagement.Application.DTOs.Books;
using LibraryManagement.Application.DTOs.Auth;
using LibraryManagement.Application.DTOs.Members;
using LibraryManagement.Application.DTOs.Reservations;
using LibraryManagement.Application.DTOs.Users;

namespace LibraryManagement.Application.Interfaces.Services;

public interface IUserService
{
    Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserDto>> GetAllAsync(UserSearchRequest request, CancellationToken cancellationToken = default);

    Task<UserDto> UpdateAsync(int userId, UpdateUserRequest request, CancellationToken cancellationToken = default);

    Task<UserDto> DeactivateAsync(int userId, CancellationToken cancellationToken = default);
}

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
}

public interface IMemberService
{
    Task<MemberDto> CreateAsync(int userId, CreateMemberRequest request, CancellationToken cancellationToken = default);

    Task<MemberDto> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MemberDto>> GetPendingAsync(CancellationToken cancellationToken = default);

    Task<MemberDto> ApproveAsync(int memberId, ApproveMemberRequest request, CancellationToken cancellationToken = default);
}

public interface IBookService
{
    Task<BookDto> CreateAsync(int addedByUserId, CreateBookRequest request, CancellationToken cancellationToken = default);

    Task<BookDto> UpdateAsync(int bookId, UpdateBookRequest request, CancellationToken cancellationToken = default);

    Task<BookDto> DeactivateAsync(int bookId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BookDto>> SearchAsync(BookSearchRequest request, CancellationToken cancellationToken = default);
}

public interface IBookIssueService
{
    Task<BookIssueDto> IssueAsync(int issuedByUserId, IssueBookRequest request, CancellationToken cancellationToken = default);

    Task<BookIssueDto> ReturnAsync(int returnedToUserId, ReturnBookRequest request, CancellationToken cancellationToken = default);

    Task<BookIssueDto> RenewAsync(RenewBookRequest request, CancellationToken cancellationToken = default);

    Task<BookIssueDto> PayFineAsync(int userId, PayFineRequest request, CancellationToken cancellationToken = default);
}

public interface IReservationService
{
    Task<ReservationDto> CreateAsync(int userId, CreateReservationRequest request, CancellationToken cancellationToken = default);

    Task<ReservationDto> CancelAsync(int userId, ReservationActionRequest request, CancellationToken cancellationToken = default);

    Task<ReservationDto> FulfillAsync(ReservationActionRequest request, CancellationToken cancellationToken = default);

    Task<int> ExpirePendingAsync(DateTime utcNow, CancellationToken cancellationToken = default);
}
