using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateReservationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    ReservationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookId = table.Column<int>(type: "int", nullable: false),
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    ReservationDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "DATEADD(DAY, 7, SYSUTCDATETIME())"),
                    FulfilledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.ReservationId);
                    table.CheckConstraint("CK_Reservations_Dates", "[ExpiryDate] >= [ReservationDate] AND ([FulfilledDate] IS NULL OR [FulfilledDate] >= [ReservationDate]) AND ([CancelledDate] IS NULL OR [CancelledDate] >= [ReservationDate])");
                    table.CheckConstraint("CK_Reservations_Lifecycle", "([Status] = 'Pending' AND [FulfilledDate] IS NULL AND [CancelledDate] IS NULL) OR ([Status] = 'Fulfilled' AND [FulfilledDate] IS NOT NULL AND [CancelledDate] IS NULL) OR ([Status] = 'Cancelled' AND [CancelledDate] IS NOT NULL AND [FulfilledDate] IS NULL) OR ([Status] = 'Expired' AND [FulfilledDate] IS NULL AND [CancelledDate] IS NULL)");
                    table.CheckConstraint("CK_Reservations_Status", "[Status] IN ('Pending', 'Fulfilled', 'Cancelled', 'Expired')");
                    table.ForeignKey(
                        name: "FK_Reservations_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservations_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_BookId",
                table: "Reservations",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_BookId_MemberId",
                table: "Reservations",
                columns: new[] { "BookId", "MemberId" },
                unique: true,
                filter: "[Status] = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ExpiryDate",
                table: "Reservations",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_MemberId",
                table: "Reservations",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ReservationDate",
                table: "Reservations",
                column: "ReservationDate");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_Status",
                table: "Reservations",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reservations");
        }
    }
}
