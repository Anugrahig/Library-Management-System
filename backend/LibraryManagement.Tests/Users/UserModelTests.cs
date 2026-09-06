using LibraryManagement.Domain.Entities;
using LibraryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LibraryManagement.Tests.Users;

public class UserModelTests
{
    [Fact]
    public void User_model_matches_database_contract()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=LibraryManagement_Test")
            .Options;

        using var context = new LibraryDbContext(options);
        var entity = context.Model.FindEntityType(typeof(User));

        Assert.NotNull(entity);
        Assert.Equal("Users", entity!.GetTableName());
        Assert.Equal(nameof(User.Id), entity.FindPrimaryKey()!.Properties.Single().Name);
        Assert.Equal(256, entity.FindProperty(nameof(User.Email))!.GetMaxLength());
        Assert.Equal(150, entity.FindProperty(nameof(User.FullName))!.GetMaxLength());
        Assert.Equal(10, entity.FindProperty(nameof(User.MobileNumber))!.GetMaxLength());
        Assert.Equal(20, entity.FindProperty(nameof(User.Role))!.GetMaxLength());
        Assert.True(entity.FindProperty(nameof(User.Email))!.IsNullable is false);
        Assert.True(entity.FindProperty(nameof(User.PasswordHash))!.IsNullable is false);
        Assert.Contains(entity.GetIndexes(), index =>
            index.IsUnique && index.Properties.Single().Name == nameof(User.Email));
    }
}
