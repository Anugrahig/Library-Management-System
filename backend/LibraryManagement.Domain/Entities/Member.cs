using LibraryManagement.Domain.Enums;

namespace LibraryManagement.Domain.Entities;

public class Member
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateTime JoiningDate { get; set; }

    public int MaxBooksAllowed { get; set; }

    public MembershipType MembershipType { get; set; }

    public bool IsApproved { get; set; }

    public string? RegistrationNumber { get; set; }

    public string? Course { get; set; }

    public string? Semester { get; set; }

    public string? BatchYear { get; set; }

    public string? EmployeeId { get; set; }

    public string? Designation { get; set; }

    public string? Department { get; set; }

    public User User { get; set; } = null!;
}
