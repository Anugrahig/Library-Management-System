using LibraryManagement.Domain.Entities;
using LibraryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LibraryManagement.Tests.Members;

public class MemberModelTests
{
    [Fact]
    public void Member_model_matches_database_contract()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=LibraryManagement_Test")
            .Options;

        using var context = new LibraryDbContext(options);
        var entity = context.Model.FindEntityType(typeof(Member));

        Assert.NotNull(entity);
        Assert.Equal("Members", entity!.GetTableName());
        Assert.Equal(nameof(Member.Id), entity.FindPrimaryKey()!.Properties.Single().Name);
        Assert.Equal("date", entity.FindProperty(nameof(Member.JoiningDate))!.GetColumnType());
        Assert.Equal(14, entity.FindProperty(nameof(Member.RegistrationNumber))!.GetMaxLength());
        Assert.Equal(30, entity.FindProperty(nameof(Member.Course))!.GetMaxLength());
        Assert.Equal(10, entity.FindProperty(nameof(Member.Semester))!.GetMaxLength());
        Assert.Equal(7, entity.FindProperty(nameof(Member.BatchYear))!.GetMaxLength());
        Assert.True(entity.FindProperty(nameof(Member.BatchYear))!.IsNullable);
        Assert.False(entity.FindProperty(nameof(Member.IsApproved))!.IsNullable);
        Assert.Equal(false, entity.FindProperty(nameof(Member.IsApproved))!.GetDefaultValue());
        Assert.Equal(8, entity.FindProperty(nameof(Member.EmployeeId))!.GetMaxLength());
        Assert.Equal(30, entity.FindProperty(nameof(Member.Designation))!.GetMaxLength());
        Assert.Equal(10, entity.FindProperty(nameof(Member.Department))!.GetMaxLength());
        Assert.Contains(entity.GetIndexes(), index =>
            index.IsUnique && index.Properties.Single().Name == nameof(Member.UserId));
        Assert.Contains(entity.GetForeignKeys(), foreignKey =>
            foreignKey.Properties.Single().Name == nameof(Member.UserId) &&
            foreignKey.PrincipalEntityType.ClrType == typeof(User));
    }
}
