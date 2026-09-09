using LibraryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagement.Infrastructure.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_Books_CopyCounts",
                "[TotalCopies] >= 0 AND [AvailableCopies] >= 0 AND [AvailableCopies] <= [TotalCopies]");

            tableBuilder.HasCheckConstraint(
                "CK_Books_Category",
                "[Category] IN ('Computer Science', 'Information Technology', 'Electronics & Communication', 'Mechanical Engineering', 'Civil Engineering', 'Mathematics', 'Physics', 'Chemistry', 'Business Administration', 'English Literature', 'General Reference', 'Fiction', 'Competitive Exam Preparation', 'Others')");
        });

        builder.HasKey(book => book.BookId);

        builder.Property(book => book.BookId)
            .ValueGeneratedOnAdd();

        builder.Property(book => book.ISBN)
            .HasMaxLength(20);

        builder.Property(book => book.Title)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(book => book.Category)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(book => book.Title);

        builder.Property(book => book.Author)
            .HasMaxLength(200);

        builder.HasIndex(book => book.Author);

        builder.Property(book => book.PublishedYear);

        builder.Property(book => book.Edition)
            .HasMaxLength(50);

        builder.Property(book => book.CoverImage)
            .HasColumnType("varchar(500)");

        builder.Property(book => book.TotalCopies)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(book => book.AvailableCopies)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(book => book.ShelfLocation)
            .HasColumnType("varchar(100)");

        builder.Property(book => book.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(book => book.CreatedAt)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Property(book => book.UpdatedAt)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Property(book => book.AddedByUserId)
            .IsRequired();

        builder.HasOne(book => book.AddedByUser)
            .WithMany()
            .HasForeignKey(book => book.AddedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
