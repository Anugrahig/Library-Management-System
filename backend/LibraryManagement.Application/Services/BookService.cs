using FluentValidation;
using LibraryManagement.Application.Common.Exceptions;
using LibraryManagement.Application.DTOs.Books;
using LibraryManagement.Application.Interfaces.Repositories;
using LibraryManagement.Application.Interfaces.Services;
using LibraryManagement.Domain.Entities;

namespace LibraryManagement.Application.Services;

public class BookService(
    IBookRepository books,
    IUserRepository users,
    IUnitOfWork unitOfWork,
    IValidator<CreateBookRequest> validator) : IBookService
{
    public async Task<BookDto> CreateAsync(CreateBookRequest request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        if (await users.GetByIdAsync(request.AddedByUserId, cancellationToken) is null)
        {
            throw new NotFoundException("The user adding the book was not found.");
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
            AvailableCopies = request.AvailableCopies,
            ShelfLocation = request.ShelfLocation,
            IsActive = true,
            AddedByUserId = request.AddedByUserId
        };

        await books.AddAsync(book, cancellationToken);
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
