using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateBookIssuesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookIssues",
                columns: table => new
                {
                    BookIssueId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookId = table.Column<int>(type: "int", nullable: false),
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "DATEADD(DAY, 14, SYSUTCDATETIME())"),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "Issued"),
                    RenewalCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    FineAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false, defaultValue: 0m),
                    FinePaid = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FinePaidDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IssuedByUserId = table.Column<int>(type: "int", nullable: false),
                    ReturnedToUserId = table.Column<int>(type: "int", nullable: true),
                    Remarks = table.Column<string>(type: "varchar(500)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookIssues", x => x.BookIssueId);
                    table.CheckConstraint("CK_BookIssues_Dates", "[DueDate] >= [IssueDate] AND [DueDate] <= DATEADD(DAY, 42, [IssueDate]) AND ([ReturnDate] IS NULL OR [ReturnDate] >= [IssueDate])");
                    table.CheckConstraint("CK_BookIssues_FinePayment", "[FineAmount] >= 0 AND (([FinePaid] = 0 AND [FinePaidDate] IS NULL) OR ([FinePaid] = 1 AND [FinePaidDate] IS NOT NULL))");
                    table.CheckConstraint("CK_BookIssues_RenewalCount", "[RenewalCount] BETWEEN 0 AND 2");
                    table.CheckConstraint("CK_BookIssues_ReturnDetails", "([Status] = 'Returned' AND [ReturnDate] IS NOT NULL AND [ReturnedToUserId] IS NOT NULL) OR ([Status] <> 'Returned' AND [ReturnDate] IS NULL AND [ReturnedToUserId] IS NULL)");
                    table.CheckConstraint("CK_BookIssues_Status", "[Status] IN ('Issued', 'Returned', 'Cancelled', 'Lost')");
                    table.ForeignKey(
                        name: "FK_BookIssues_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookIssues_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookIssues_Users_IssuedByUserId",
                        column: x => x.IssuedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookIssues_Users_ReturnedToUserId",
                        column: x => x.ReturnedToUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookIssues_BookId",
                table: "BookIssues",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_BookIssues_IssueDate",
                table: "BookIssues",
                column: "IssueDate");

            migrationBuilder.CreateIndex(
                name: "IX_BookIssues_IssuedByUserId",
                table: "BookIssues",
                column: "IssuedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BookIssues_MemberId",
                table: "BookIssues",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_BookIssues_ReturnedToUserId",
                table: "BookIssues",
                column: "ReturnedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BookIssues_Status",
                table: "BookIssues",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookIssues");
        }
    }
}
