using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBatchYearToMembers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Members_StudentFields",
                table: "Members");

            migrationBuilder.AddColumn<string>(
                name: "BatchYear",
                table: "Members",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Members_StudentBatchYear",
                table: "Members",
                sql: "[MembershipType] <> 'Student' OR ((([Course] IN ('MCA', 'MBA') OR [Course] LIKE 'MTech %') AND [BatchYear] IN ('2024-26', '2025-27', '2026-28')) OR ([Course] LIKE 'BTech %' AND [BatchYear] IN ('2022-26', '2023-27', '2024-28', '2025-29', '2026-30')))");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Members_StudentFields",
                table: "Members",
                sql: "([MembershipType] = 'Student' AND [RegistrationNumber] IS NOT NULL AND LEN([RegistrationNumber]) BETWEEN 12 AND 14 AND [Course] IS NOT NULL AND [Semester] IS NOT NULL AND [BatchYear] IS NOT NULL AND [EmployeeId] IS NULL AND [Designation] IS NULL AND [Department] IS NULL) OR ([MembershipType] = 'Faculty' AND [EmployeeId] IS NOT NULL AND LEN([EmployeeId]) BETWEEN 6 AND 8 AND [Designation] IS NOT NULL AND [Department] IS NOT NULL AND [RegistrationNumber] IS NULL AND [Course] IS NULL AND [Semester] IS NULL AND [BatchYear] IS NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Members_StudentBatchYear",
                table: "Members");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Members_StudentFields",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "BatchYear",
                table: "Members");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Members_StudentFields",
                table: "Members",
                sql: "([MembershipType] = 'Student' AND [RegistrationNumber] IS NOT NULL AND LEN([RegistrationNumber]) BETWEEN 12 AND 14 AND [Course] IS NOT NULL AND [Semester] IS NOT NULL AND [EmployeeId] IS NULL AND [Designation] IS NULL AND [Department] IS NULL) OR ([MembershipType] = 'Faculty' AND [EmployeeId] IS NOT NULL AND LEN([EmployeeId]) BETWEEN 6 AND 8 AND [Designation] IS NOT NULL AND [Department] IS NOT NULL AND [RegistrationNumber] IS NULL AND [Course] IS NULL AND [Semester] IS NULL)");
        }
    }
}
