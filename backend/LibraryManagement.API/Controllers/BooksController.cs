using System.Security.Claims;
using LibraryManagement.Application.DTOs.Books;
using LibraryManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers;

[ApiController]
[Route("api/books")]
[Authorize]
public class BooksController(IBookService bookService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookDto>>> Search(
        [FromQuery] BookSearchRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await bookService.SearchAsync(request, cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<ActionResult<BookDto>> Create(
        CreateBookRequest request,
        CancellationToken cancellationToken)
    {
        var book = await bookService.CreateAsync(GetCurrentUserId(), request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, book);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<ActionResult<BookDto>> Update(
        int id,
        UpdateBookRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await bookService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpPatch("{id:int}/deactivate")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<ActionResult<BookDto>> Deactivate(
        int id,
        CancellationToken cancellationToken)
    {
        return Ok(await bookService.DeactivateAsync(id, cancellationToken));
    }

    private int GetCurrentUserId()
    {
        var value = User.FindFirstValue("UserId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(value, out var userId))
        {
            throw new UnauthorizedAccessException("The authenticated user ID claim is missing.");
        }

        return userId;
    }
}
