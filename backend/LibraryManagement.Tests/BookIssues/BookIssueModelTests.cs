using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Enums;
using LibraryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LibraryManagement.Tests.BookIssues;

public class BookIssueModelTests
{
    [Fact]
    public void Book_issue_model_matches_database_contract()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=LibraryManagement_Test")
            .Options;

        using var context = new LibraryDbContext(options);
        var entity = context.Model.FindEntityType(typeof(BookIssue));

        Assert.NotNull(entity);
        Assert.Equal("BookIssues", entity!.GetTableName());
        Assert.Equal(nameof(BookIssue.BookIssueId), entity.FindPrimaryKey()!.Properties.Single().Name);
        Assert.Equal("varchar(20)", entity.FindProperty(nameof(BookIssue.Status))!.GetColumnType());
        Assert.Equal("decimal(10,2)", entity.FindProperty(nameof(BookIssue.FineAmount))!.GetColumnType());
        Assert.Equal("varchar(500)", entity.FindProperty(nameof(BookIssue.Remarks))!.GetColumnType());
        Assert.True(entity.FindProperty(nameof(BookIssue.ReturnDate))!.IsNullable);
        Assert.True(entity.FindProperty(nameof(BookIssue.ReturnedToUserId))!.IsNullable);
        Assert.Equal(0, entity.FindProperty(nameof(BookIssue.RenewalCount))!.GetDefaultValue());
        Assert.Equal(0m, entity.FindProperty(nameof(BookIssue.FineAmount))!.GetDefaultValue());
        Assert.Equal(false, entity.FindProperty(nameof(BookIssue.FinePaid))!.GetDefaultValue());
        Assert.Equal(BookIssueStatus.Issued, entity.FindProperty(nameof(BookIssue.Status))!.GetDefaultValue());
        Assert.Contains(entity.GetForeignKeys(), foreignKey =>
            foreignKey.Properties.Single().Name == nameof(BookIssue.BookId) &&
            foreignKey.PrincipalEntityType.ClrType == typeof(Book));
        Assert.Contains(entity.GetForeignKeys(), foreignKey =>
            foreignKey.Properties.Single().Name == nameof(BookIssue.MemberId) &&
            foreignKey.PrincipalEntityType.ClrType == typeof(Member));
        Assert.Equal(4, entity.GetForeignKeys().Count());
    }
}
