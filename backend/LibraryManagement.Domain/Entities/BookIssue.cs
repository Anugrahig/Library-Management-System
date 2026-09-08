using LibraryManagement.Domain.Enums;

namespace LibraryManagement.Domain.Entities;

public class BookIssue
{
    public int BookIssueId { get; set; }

    public int BookId { get; set; }

    public int MemberId { get; set; }

    public DateTime IssueDate { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    public BookIssueStatus Status { get; set; } = BookIssueStatus.Issued;

    public int RenewalCount { get; set; }

    public decimal FineAmount { get; set; }

    public bool FinePaid { get; set; }

    public DateTime? FinePaidDate { get; set; }

    public int IssuedByUserId { get; set; }

    public int? ReturnedToUserId { get; set; }

    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Book Book { get; set; } = null!;

    public Member Member { get; set; } = null!;

    public User IssuedByUser { get; set; } = null!;

    public User? ReturnedToUser { get; set; }
}
