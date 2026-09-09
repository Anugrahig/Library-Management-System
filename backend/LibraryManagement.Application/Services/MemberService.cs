using FluentValidation;
using LibraryManagement.Application.Common.Exceptions;
using LibraryManagement.Application.DTOs.Members;
using LibraryManagement.Application.Interfaces.Repositories;
using LibraryManagement.Application.Interfaces.Services;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Enums;

namespace LibraryManagement.Application.Services;

public class MemberService(
    IUserRepository users,
    IMemberRepository members,
    IUnitOfWork unitOfWork,
    IValidator<CreateMemberRequest> validator) : IMemberService
{
    private static readonly string[] StudentCourses =
    [
        "BTech CSE", "BTech IT", "BTech ECE", "BTech EEE", "BTech ME", "BTech CE",
        "MCA", "MBA", "MTech CSE", "MTech IT", "MTech ECE", "MTech EEE", "MTech ME", "MTech CE"
    ];

    private static readonly string[] FacultyDesignations =
    [
        "Professor", "Associate Professor", "Assistant Professor", "Lecturer", "Senior Lecturer",
        "Visiting Faculty", "Guest Faculty", "Adjunct Faculty", "HOD", "Dean"
    ];

    private static readonly string[] FacultyDepartments = ["CSE", "ECE", "EEE", "ME", "CE", "IT", "MBA", "MCA"];

    public async Task<MemberDto> CreateAsync(CreateMemberRequest request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("The linked user was not found.");

        if (user.Role is not (UserRole.Student or UserRole.Faculty))
        {
            throw new BusinessRuleException("Only Student and Faculty users can become members.");
        }

        if (await members.GetByUserIdAsync(request.UserId, cancellationToken) is not null)
        {
            throw new ConflictException("This user already has a member profile.");
        }

        var member = user.Role == UserRole.Student
            ? BuildStudent(request)
            : BuildFaculty(request);

        await members.AddAsync(member, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(member);
    }

    public async Task<MemberDto> ApproveAsync(int memberId, ApproveMemberRequest request, CancellationToken cancellationToken = default)
    {
        var member = await members.GetByIdAsync(memberId, cancellationToken)
            ?? throw new NotFoundException("The member was not found.");

        member.IsApproved = request.IsApproved;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(member);
    }

    private static Member BuildStudent(CreateMemberRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RegistrationNumber) ||
            string.IsNullOrWhiteSpace(request.Course) ||
            string.IsNullOrWhiteSpace(request.Semester) ||
            string.IsNullOrWhiteSpace(request.BatchYear))
        {
            throw new BusinessRuleException("Registration number, course, semester, and batch year are required for students.");
        }

        if (!StudentCourses.Contains(request.Course, StringComparer.Ordinal))
        {
            throw new BusinessRuleException("The selected course is invalid.");
        }

        return new Member
        {
            UserId = request.UserId,
            JoiningDate = request.JoiningDate.Date,
            MaxBooksAllowed = 5,
            MembershipType = MembershipType.Student,
            IsApproved = false,
            RegistrationNumber = request.RegistrationNumber,
            Course = request.Course,
            Semester = request.Semester,
            BatchYear = request.BatchYear
        };
    }

    private static Member BuildFaculty(CreateMemberRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.EmployeeId) ||
            string.IsNullOrWhiteSpace(request.Designation) ||
            string.IsNullOrWhiteSpace(request.Department))
        {
            throw new BusinessRuleException("Employee ID, designation, and department are required for faculty members.");
        }

        if (!FacultyDesignations.Contains(request.Designation, StringComparer.Ordinal) ||
            !FacultyDepartments.Contains(request.Department, StringComparer.Ordinal))
        {
            throw new BusinessRuleException("The faculty designation or department is invalid.");
        }

        return new Member
        {
            UserId = request.UserId,
            JoiningDate = request.JoiningDate.Date,
            MaxBooksAllowed = 8,
            MembershipType = MembershipType.Faculty,
            IsApproved = false,
            EmployeeId = request.EmployeeId,
            Designation = request.Designation,
            Department = request.Department
        };
    }

    private static MemberDto Map(Member member) => new()
    {
        Id = member.Id,
        UserId = member.UserId,
        JoiningDate = member.JoiningDate,
        MaxBooksAllowed = member.MaxBooksAllowed,
        MembershipType = member.MembershipType,
        IsApproved = member.IsApproved,
        RegistrationNumber = member.RegistrationNumber,
        Course = member.Course,
        Semester = member.Semester,
        BatchYear = member.BatchYear,
        EmployeeId = member.EmployeeId,
        Designation = member.Designation,
        Department = member.Department
    };
}
