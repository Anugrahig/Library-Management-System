using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagement.Infrastructure.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservations", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_Reservations_Status",
                "[Status] IN ('Pending', 'Fulfilled', 'Cancelled', 'Expired')");

            tableBuilder.HasCheckConstraint(
                "CK_Reservations_Dates",
                "[ExpiryDate] >= [ReservationDate] AND ([FulfilledDate] IS NULL OR [FulfilledDate] >= [ReservationDate]) AND ([CancelledDate] IS NULL OR [CancelledDate] >= [ReservationDate])");

            tableBuilder.HasCheckConstraint(
                "CK_Reservations_Lifecycle",
                "([Status] = 'Pending' AND [FulfilledDate] IS NULL AND [CancelledDate] IS NULL) OR ([Status] = 'Fulfilled' AND [FulfilledDate] IS NOT NULL AND [CancelledDate] IS NULL) OR ([Status] = 'Cancelled' AND [CancelledDate] IS NOT NULL AND [FulfilledDate] IS NULL) OR ([Status] = 'Expired' AND [FulfilledDate] IS NULL AND [CancelledDate] IS NULL)");
        });

        builder.HasKey(reservation => reservation.ReservationId);

        builder.Property(reservation => reservation.ReservationId)
            .ValueGeneratedOnAdd();

        builder.Property(reservation => reservation.BookId)
            .IsRequired();

        builder.HasIndex(reservation => reservation.BookId);

        builder.HasOne(reservation => reservation.Book)
            .WithMany()
            .HasForeignKey(reservation => reservation.BookId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(reservation => reservation.MemberId)
            .IsRequired();

        builder.HasIndex(reservation => reservation.MemberId);

        builder.HasOne(reservation => reservation.Member)
            .WithMany()
            .HasForeignKey(reservation => reservation.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(reservation => reservation.ReservationDate)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.HasIndex(reservation => reservation.ReservationDate);

        builder.Property(reservation => reservation.ExpiryDate)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("DATEADD(DAY, 7, SYSUTCDATETIME())")
            .IsRequired();

        builder.HasIndex(reservation => reservation.ExpiryDate);

        builder.Property(reservation => reservation.FulfilledDate)
            .HasColumnType("datetime2");

        builder.Property(reservation => reservation.CancelledDate)
            .HasColumnType("datetime2");

        builder.Property(reservation => reservation.Status)
            .HasConversion<string>()
            .HasColumnType("nvarchar(20)")
            .HasMaxLength(20)
            .HasDefaultValue(ReservationStatus.Pending)
            .IsRequired();

        builder.HasIndex(reservation => reservation.Status);

        builder.HasIndex(reservation => new { reservation.BookId, reservation.MemberId })
            .IsUnique()
            .HasFilter("[Status] = 'Pending'");

        builder.Property(reservation => reservation.CreatedAt)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Property(reservation => reservation.UpdatedAt)
            .HasColumnType("datetime2");
    }
}
