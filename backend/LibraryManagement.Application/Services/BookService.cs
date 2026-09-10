using FluentValidation;
using LibraryManagement.Application.Common.Exceptions;
using LibraryManagement.Application.DTOs.Books;
using LibraryManagement.Application.Interfaces.Repositories;
using LibraryManagement.Application.Interfaces.Services;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Enums;

namespace LibraryManagement.Application.Services;

public class BookService(
    IBookRepository books,
    IUserRepository users,
    IBookIssueRepository issues,
    IUnitOfWork unitOfWork,
    IValidator<CreateBookRequest> validator) : IBookService
{
    public async Task<BookDto> CreateAsync(int addedByUserId, CreateBookRequest request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await users.GetByIdAsync(addedByUserId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("The user adding the book was not found.");
        }

        if (user.Role is not (UserRole.Admin or UserRole.Librarian))
        {
            throw new BusinessRuleException("Only Admin or Librarian users can manage books.");
        }

        var book = new Book
        {
            ISBN = request.ISBN,
            Title = request.Title.Trim(),
            Category = request.Category,
            Author = request.Author,
            PublishedYear = request.PublishedYear,
            Edition = request.Edition,
            CoverImage = request.CoverImage,
            TotalCopies = request.TotalCopies,
            AvailableCopies = request.TotalCopies,
            ShelfLocation = request.ShelfLocation,
            IsActive = true,
            AddedByUserId = addedByUserId
        };

        await books.AddAsync(book, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(book);
    }

    public async Task<BookDto> UpdateAsync(int bookId, UpdateBookRequest request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var book = await books.GetByIdAsync(bookId, cancellationToken)
            ?? throw new NotFoundException("The book was not found.");
        var issuedCopies = await issues.CountActiveForBookAsync(bookId, cancellationToken);

        if (request.TotalCopies < issuedCopies)
        {
            throw new BusinessRuleException("Total copies cannot be less than currently issued copies.");
        }

        book.ISBN = request.ISBN;
        book.Title = request.Title.Trim();
        book.Category = request.Category;
        book.Author = request.Author;
        book.PublishedYear = request.PublishedYear;
        book.Edition = request.Edition;
        book.CoverImage = request.CoverImage;
        book.TotalCopies = request.TotalCopies;
        book.AvailableCopies = request.TotalCopies - issuedCopies;
        book.ShelfLocation = request.ShelfLocation;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(book);
    }

    public async Task<BookDto> DeactivateAsync(int bookId, CancellationToken cancellationToken = default)
    {
        var book = await books.GetByIdAsync(bookId, cancellationToken)
            ?? throw new NotFoundException("The book was not found.");

        book.IsActive = false;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(book);
    }

    public async Task<IReadOnlyList<BookDto>> SearchAsync(BookSearchRequest request, CancellationToken cancellationToken = default)
    {
        var results = await books.SearchAsync(request.SearchTerm, request.Category, cancellationToken);
        return results.Select(Map).ToList();
    }

    private static BookDto Map(Book book) => new()
    {
        BookId = book.BookId,
        ISBN = book.ISBN,
        Title = book.Title,
        Category = book.Category,
        Author = book.Author,
        PublishedYear = book.PublishedYear,
        Edition = book.Edition,
        CoverImage = book.CoverImage,
        TotalCopies = book.TotalCopies,
        AvailableCopies = book.AvailableCopies,
        ShelfLocation = book.ShelfLocation,
        IsActive = book.IsActive,
        CreatedAt = book.CreatedAt,
        UpdatedAt = book.UpdatedAt,
        AddedByUserId = book.AddedByUserId
    };
}
