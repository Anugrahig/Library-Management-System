using LibraryManagement.Application.DTOs.BookIssues;
using LibraryManagement.Application.DTOs.Books;
using LibraryManagement.Application.DTOs.Members;
using LibraryManagement.Application.DTOs.Reservations;
using LibraryManagement.Application.DTOs.Users;

namespace LibraryManagement.Application.Interfaces.Services;

public interface IUserService
{
    Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
}

public interface IMemberService
{
    Task<MemberDto> CreateAsync(CreateMemberRequest request, CancellationToken cancellationToken = default);

    Task<MemberDto> ApproveAsync(int memberId, ApproveMemberRequest request, CancellationToken cancellationToken = default);
}

public interface IBookService
{
    Task<BookDto> CreateAsync(CreateBookRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BookDto>> SearchAsync(BookSearchRequest request, CancellationToken cancellationToken = default);
}

public interface IBookIssueService
{
    Task<BookIssueDto> IssueAsync(IssueBookRequest request, CancellationToken cancellationToken = default);

    Task<BookIssueDto> ReturnAsync(ReturnBookRequest request, CancellationToken cancellationToken = default);

    Task<BookIssueDto> RenewAsync(RenewBookRequest request, CancellationToken cancellationToken = default);
}

public interface IReservationService
{
    Task<ReservationDto> CreateAsync(CreateReservationRequest request, CancellationToken cancellationToken = default);

    Task<ReservationDto> CancelAsync(ReservationActionRequest request, CancellationToken cancellationToken = default);

    Task<ReservationDto> FulfillAsync(ReservationActionRequest request, CancellationToken cancellationToken = default);
}
