using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Enums;
using LibraryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LibraryManagement.Tests.Reservations;

public class ReservationModelTests
{
    [Fact]
    public void Reservation_model_matches_database_contract()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=LibraryManagement_Test")
            .Options;

        using var context = new LibraryDbContext(options);
        var entity = context.Model.FindEntityType(typeof(Reservation));

        Assert.NotNull(entity);
        Assert.Equal("Reservations", entity!.GetTableName());
        Assert.Equal(nameof(Reservation.ReservationId), entity.FindPrimaryKey()!.Properties.Single().Name);
        Assert.Equal("nvarchar(20)", entity.FindProperty(nameof(Reservation.Status))!.GetColumnType());
        Assert.Equal(ReservationStatus.Pending, entity.FindProperty(nameof(Reservation.Status))!.GetDefaultValue());
        Assert.True(entity.FindProperty(nameof(Reservation.FulfilledDate))!.IsNullable);
        Assert.True(entity.FindProperty(nameof(Reservation.CancelledDate))!.IsNullable);
        Assert.True(entity.FindProperty(nameof(Reservation.UpdatedAt))!.IsNullable);
        Assert.NotNull(entity.FindProperty(nameof(Reservation.ReservationDate))!.GetDefaultValueSql());
        Assert.Contains("DATEADD", entity.FindProperty(nameof(Reservation.ExpiryDate))!.GetDefaultValueSql());
        Assert.Contains(entity.GetIndexes(), index =>
            index.IsUnique &&
            index.Properties.Select(property => property.Name).SequenceEqual(
                new[] { nameof(Reservation.BookId), nameof(Reservation.MemberId) }) &&
            index.GetFilter() == "[Status] = 'Pending'");
        Assert.Contains(entity.GetForeignKeys(), foreignKey =>
            foreignKey.Properties.Single().Name == nameof(Reservation.BookId) &&
            foreignKey.PrincipalEntityType.ClrType == typeof(Book));
        Assert.Contains(entity.GetForeignKeys(), foreignKey =>
            foreignKey.Properties.Single().Name == nameof(Reservation.MemberId) &&
            foreignKey.PrincipalEntityType.ClrType == typeof(Member));
    }
}
