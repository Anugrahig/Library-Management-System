namespace LibraryManagement.Domain.Entities;

public class Book
{
    public int BookId { get; set; }

    public string? ISBN { get; set; }

    public required string Title { get; set; }

    public required string Category { get; set; }

    public string? Author { get; set; }

    public int? PublishedYear { get; set; }

    public string? Edition { get; set; }

    public string? CoverImage { get; set; }

    public int TotalCopies { get; set; }

    public int AvailableCopies { get; set; }

    public string? ShelfLocation { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int AddedByUserId { get; set; }

    public User AddedByUser { get; set; } = null!;
}
