namespace LibraryManagement.Application.DTOs.Books;

public class CreateBookRequest
{
    public string? ISBN { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string? Author { get; set; }

    public int? PublishedYear { get; set; }

    public string? Edition { get; set; }

    public string? CoverImage { get; set; }

    public int TotalCopies { get; set; }

    public string? ShelfLocation { get; set; }
}

public class UpdateBookRequest : CreateBookRequest
{
}

public class BookSearchRequest
{
    public string? SearchTerm { get; set; }

    public string? Category { get; set; }
}

public class BookDto
{
    public int BookId { get; set; }

    public string? ISBN { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string? Author { get; set; }

    public int? PublishedYear { get; set; }

    public string? Edition { get; set; }

    public string? CoverImage { get; set; }

    public int TotalCopies { get; set; }

    public int AvailableCopies { get; set; }

    public string? ShelfLocation { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int AddedByUserId { get; set; }
}
