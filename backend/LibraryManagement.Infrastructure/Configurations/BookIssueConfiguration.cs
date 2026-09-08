using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagement.Infrastructure.Configurations;

public class BookIssueConfiguration : IEntityTypeConfiguration<BookIssue>
{
    public void Configure(EntityTypeBuilder<BookIssue> builder)
    {
        builder.ToTable("BookIssues", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_BookIssues_Status",
                "[Status] IN ('Issued', 'Returned', 'Cancelled', 'Lost')");

            tableBuilder.HasCheckConstraint(
                "CK_BookIssues_RenewalCount",
                "[RenewalCount] BETWEEN 0 AND 2");

            tableBuilder.HasCheckConstraint(
                "CK_BookIssues_Dates",
                "[DueDate] >= [IssueDate] AND [DueDate] <= DATEADD(DAY, 42, [IssueDate]) AND ([ReturnDate] IS NULL OR [ReturnDate] >= [IssueDate])");

            tableBuilder.HasCheckConstraint(
                "CK_BookIssues_ReturnDetails",
                "([Status] = 'Returned' AND [ReturnDate] IS NOT NULL AND [ReturnedToUserId] IS NOT NULL) OR ([Status] <> 'Returned' AND [ReturnDate] IS NULL AND [ReturnedToUserId] IS NULL)");

            tableBuilder.HasCheckConstraint(
                "CK_BookIssues_FinePayment",
                "[FineAmount] >= 0 AND (([FinePaid] = 0 AND [FinePaidDate] IS NULL) OR ([FinePaid] = 1 AND [FinePaidDate] IS NOT NULL))");
        });

        builder.HasKey(issue => issue.BookIssueId);

        builder.Property(issue => issue.BookIssueId)
            .ValueGeneratedOnAdd();

        builder.Property(issue => issue.BookId)
            .IsRequired();

        builder.HasIndex(issue => issue.BookId);

        builder.HasOne(issue => issue.Book)
            .WithMany()
            .HasForeignKey(issue => issue.BookId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(issue => issue.MemberId)
            .IsRequired();

        builder.HasIndex(issue => issue.MemberId);

        builder.HasOne(issue => issue.Member)
            .WithMany()
            .HasForeignKey(issue => issue.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(issue => issue.IssueDate)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.HasIndex(issue => issue.IssueDate);

        builder.Property(issue => issue.DueDate)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("DATEADD(DAY, 14, SYSUTCDATETIME())")
            .IsRequired();

        builder.Property(issue => issue.ReturnDate)
            .HasColumnType("datetime2");

        builder.Property(issue => issue.Status)
            .HasConversion<string>()
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .HasDefaultValue(BookIssueStatus.Issued)
            .IsRequired();

        builder.HasIndex(issue => issue.Status);

        builder.Property(issue => issue.RenewalCount)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(issue => issue.FineAmount)
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(0m)
            .IsRequired();

        builder.Property(issue => issue.FinePaid)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(issue => issue.FinePaidDate)
            .HasColumnType("datetime2");

        builder.Property(issue => issue.IssuedByUserId)
            .IsRequired();

        builder.HasIndex(issue => issue.IssuedByUserId);

        builder.HasOne(issue => issue.IssuedByUser)
            .WithMany()
            .HasForeignKey(issue => issue.IssuedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(issue => issue.ReturnedToUserId);

        builder.HasOne(issue => issue.ReturnedToUser)
            .WithMany()
            .HasForeignKey(issue => issue.ReturnedToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(issue => issue.Remarks)
            .HasColumnType("varchar(500)");

        builder.Property(issue => issue.CreatedAt)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Property(issue => issue.UpdatedAt)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();
    }
}
