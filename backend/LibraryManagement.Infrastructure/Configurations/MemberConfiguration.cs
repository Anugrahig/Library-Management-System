using LibraryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryManagement.Infrastructure.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("Members", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_Members_MembershipType",
                "[MembershipType] IN ('Student', 'Faculty')");

            tableBuilder.HasCheckConstraint(
                "CK_Members_MaxBooksAllowed",
                "([MembershipType] = 'Student' AND [MaxBooksAllowed] = 5) OR ([MembershipType] = 'Faculty' AND [MaxBooksAllowed] = 8)");

            tableBuilder.HasCheckConstraint(
                "CK_Members_StudentFields",
                "([MembershipType] = 'Student' AND [RegistrationNumber] IS NOT NULL AND LEN([RegistrationNumber]) BETWEEN 12 AND 14 AND [Course] IS NOT NULL AND [Semester] IS NOT NULL AND [BatchYear] IS NOT NULL AND [EmployeeId] IS NULL AND [Designation] IS NULL AND [Department] IS NULL) OR ([MembershipType] = 'Faculty' AND [EmployeeId] IS NOT NULL AND LEN([EmployeeId]) BETWEEN 6 AND 8 AND [Designation] IS NOT NULL AND [Department] IS NOT NULL AND [RegistrationNumber] IS NULL AND [Course] IS NULL AND [Semester] IS NULL AND [BatchYear] IS NULL)");

            tableBuilder.HasCheckConstraint(
                "CK_Members_StudentCourse",
                "[MembershipType] <> 'Student' OR ([Course] IN ('BTech CSE', 'BTech IT', 'BTech ECE', 'BTech EEE', 'BTech ME', 'BTech CE', 'MCA', 'MBA', 'MTech CSE', 'MTech IT', 'MTech ECE', 'MTech EEE', 'MTech ME', 'MTech CE') AND (([Course] IN ('MCA', 'MBA') AND [Semester] IN ('I', 'II', 'III', 'IV')) OR ([Course] LIKE 'BTech %' AND [Semester] IN ('I', 'II', 'III', 'IV', 'V', 'VI', 'VII', 'VIII')) OR ([Course] LIKE 'MTech %' AND [Semester] IN ('I', 'II', 'III', 'IV'))))");

            tableBuilder.HasCheckConstraint(
                "CK_Members_StudentBatchYear",
                "[MembershipType] <> 'Student' OR ((([Course] IN ('MCA', 'MBA') OR [Course] LIKE 'MTech %') AND [BatchYear] IN ('2024-26', '2025-27', '2026-28')) OR ([Course] LIKE 'BTech %' AND [BatchYear] IN ('2022-26', '2023-27', '2024-28', '2025-29', '2026-30')))");

            tableBuilder.HasCheckConstraint(
                "CK_Members_FacultyFields",
                "[MembershipType] <> 'Faculty' OR ([Designation] IN ('Professor', 'Associate Professor', 'Assistant Professor', 'Lecturer', 'Senior Lecturer', 'Visiting Faculty', 'Guest Faculty', 'Adjunct Faculty', 'HOD', 'Dean') AND [Department] IN ('CSE', 'ECE', 'EEE', 'ME', 'CE', 'IT', 'MBA', 'MCA'))");
        });

        builder.HasKey(member => member.Id);

        builder.Property(member => member.Id)
            .ValueGeneratedOnAdd();

        builder.Property(member => member.UserId)
            .IsRequired();

        builder.HasIndex(member => member.UserId)
            .IsUnique();

        builder.HasOne(member => member.User)
            .WithOne(user => user.Member)
            .HasForeignKey<Member>(member => member.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(member => member.JoiningDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(member => member.MaxBooksAllowed)
            .IsRequired();

        builder.Property(member => member.MembershipType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(member => member.IsApproved)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(member => member.RegistrationNumber)
            .HasMaxLength(14);

        builder.HasIndex(member => member.RegistrationNumber)
            .IsUnique()
            .HasFilter("[RegistrationNumber] IS NOT NULL");

        builder.Property(member => member.Course)
            .HasMaxLength(30);

        builder.Property(member => member.Semester)
            .HasMaxLength(10);

        builder.Property(member => member.BatchYear)
            .HasMaxLength(7);

        builder.Property(member => member.EmployeeId)
            .HasMaxLength(8);

        builder.HasIndex(member => member.EmployeeId)
            .IsUnique()
            .HasFilter("[EmployeeId] IS NOT NULL");

        builder.Property(member => member.Designation)
            .HasMaxLength(30);

        builder.Property(member => member.Department)
            .HasMaxLength(10);
    }
}
