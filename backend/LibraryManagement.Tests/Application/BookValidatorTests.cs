using LibraryManagement.Application.DTOs.Books;
using LibraryManagement.Application.Validators.Books;
using Xunit;

namespace LibraryManagement.Tests.Application;

public class BookValidatorTests
{
    [Fact]
    public void Book_validator_rejects_unknown_category()
    {
        var request = new CreateBookRequest
        {
            Title = "Distributed Systems",
            Category = "Unknown Category"
        };

        var result = new CreateBookRequestValidator().Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateBookRequest.Category));
    }
}
