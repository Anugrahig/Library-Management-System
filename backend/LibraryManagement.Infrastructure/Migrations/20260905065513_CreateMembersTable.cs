using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateMembersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    JoiningDate = table.Column<DateTime>(type: "date", nullable: false),
                    MaxBooksAllowed = table.Column<int>(type: "int", nullable: false),
                    MembershipType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: true),
                    Course = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Semester = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    EmployeeId = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    Designation = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Department = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
                    table.CheckConstraint("CK_Members_FacultyFields", "[MembershipType] <> 'Faculty' OR ([Designation] IN ('Professor', 'Associate Professor', 'Assistant Professor', 'Lecturer', 'Senior Lecturer', 'Visiting Faculty', 'Guest Faculty', 'Adjunct Faculty', 'HOD', 'Dean') AND [Department] IN ('CSE', 'ECE', 'EEE', 'ME', 'CE', 'IT', 'MBA', 'MCA'))");
                    table.CheckConstraint("CK_Members_MaxBooksAllowed", "([MembershipType] = 'Student' AND [MaxBooksAllowed] = 5) OR ([MembershipType] = 'Faculty' AND [MaxBooksAllowed] = 8)");
                    table.CheckConstraint("CK_Members_MembershipType", "[MembershipType] IN ('Student', 'Faculty')");
                    table.CheckConstraint("CK_Members_StudentCourse", "[MembershipType] <> 'Student' OR ([Course] IN ('BTech CSE', 'BTech IT', 'BTech ECE', 'BTech EEE', 'BTech ME', 'BTech CE', 'MCA', 'MBA', 'MTech CSE', 'MTech IT', 'MTech ECE', 'MTech EEE', 'MTech ME', 'MTech CE') AND (([Course] IN ('MCA', 'MBA') AND [Semester] IN ('I', 'II', 'III', 'IV')) OR ([Course] LIKE 'BTech %' AND [Semester] IN ('I', 'II', 'III', 'IV', 'V', 'VI', 'VII', 'VIII')) OR ([Course] LIKE 'MTech %' AND [Semester] IN ('I', 'II', 'III', 'IV'))))");
                    table.CheckConstraint("CK_Members_StudentFields", "([MembershipType] = 'Student' AND [RegistrationNumber] IS NOT NULL AND LEN([RegistrationNumber]) BETWEEN 12 AND 14 AND [Course] IS NOT NULL AND [Semester] IS NOT NULL AND [EmployeeId] IS NULL AND [Designation] IS NULL AND [Department] IS NULL) OR ([MembershipType] = 'Faculty' AND [EmployeeId] IS NOT NULL AND LEN([EmployeeId]) BETWEEN 6 AND 8 AND [Designation] IS NOT NULL AND [Department] IS NOT NULL AND [RegistrationNumber] IS NULL AND [Course] IS NULL AND [Semester] IS NULL)");
                    table.ForeignKey(
                        name: "FK_Members_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Members_EmployeeId",
                table: "Members",
                column: "EmployeeId",
                unique: true,
                filter: "[EmployeeId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Members_RegistrationNumber",
                table: "Members",
                column: "RegistrationNumber",
                unique: true,
                filter: "[RegistrationNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Members_UserId",
                table: "Members",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Members");
        }
    }
}
