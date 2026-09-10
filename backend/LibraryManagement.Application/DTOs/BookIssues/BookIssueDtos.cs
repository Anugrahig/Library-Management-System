namespace LibraryManagement.Application.DTOs.BookIssues;

public class IssueBookRequest
{
    public int BookId { get; set; }

    public int MemberId { get; set; }
}

public class ReturnBookRequest
{
    public int BookIssueId { get; set; }

    public decimal FineAmount { get; set; }
}

public class RenewBookRequest
{
    public int BookIssueId { get; set; }
}

public class PayFineRequest
{
    public int BookIssueId { get; set; }
}

public class BookIssueDto
{
    public int BookIssueId { get; set; }

    public int BookId { get; set; }

    public int MemberId { get; set; }

    public DateTime IssueDate { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public int RenewalCount { get; set; }

    public decimal FineAmount { get; set; }

    public bool FinePaid { get; set; }

    public DateTime? FinePaidDate { get; set; }

    public int IssuedByUserId { get; set; }

    public int? ReturnedToUserId { get; set; }

    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
