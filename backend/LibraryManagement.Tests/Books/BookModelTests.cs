using LibraryManagement.Domain.Entities;
using LibraryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LibraryManagement.Tests.Books;

public class BookModelTests
{
    [Fact]
    public void Book_model_matches_database_contract()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=LibraryManagement_Test")
            .Options;

        using var context = new LibraryDbContext(options);
        var entity = context.Model.FindEntityType(typeof(Book));

        Assert.NotNull(entity);
        Assert.Equal("Books", entity!.GetTableName());
        Assert.Equal(nameof(Book.BookId), entity.FindPrimaryKey()!.Properties.Single().Name);
        Assert.False(entity.FindProperty(nameof(Book.Title))!.IsNullable);
        Assert.Equal(250, entity.FindProperty(nameof(Book.Title))!.GetMaxLength());
        Assert.Equal(20, entity.FindProperty(nameof(Book.ISBN))!.GetMaxLength());
        Assert.Equal(200, entity.FindProperty(nameof(Book.Author))!.GetMaxLength());
        Assert.Equal("varchar(500)", entity.FindProperty(nameof(Book.CoverImage))!.GetColumnType());
        Assert.Equal("varchar(100)", entity.FindProperty(nameof(Book.ShelfLocation))!.GetColumnType());
        Assert.Equal(0, entity.FindProperty(nameof(Book.TotalCopies))!.GetDefaultValue());
        Assert.Equal(0, entity.FindProperty(nameof(Book.AvailableCopies))!.GetDefaultValue());
        Assert.Equal(true, entity.FindProperty(nameof(Book.IsActive))!.GetDefaultValue());
        Assert.Contains(entity.GetIndexes(), index =>
            !index.IsUnique && index.Properties.Single().Name == nameof(Book.Title));
        Assert.Contains(entity.GetIndexes(), index =>
            !index.IsUnique && index.Properties.Single().Name == nameof(Book.Author));
        Assert.Contains(entity.GetForeignKeys(), foreignKey =>
            foreignKey.Properties.Single().Name == nameof(Book.AddedByUserId) &&
            foreignKey.PrincipalEntityType.ClrType == typeof(User));
    }
}
