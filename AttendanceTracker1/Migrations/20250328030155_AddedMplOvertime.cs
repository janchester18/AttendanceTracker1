using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendanceTracker1.Migrations
{
    /// <inheritdoc />
    public partial class AddedMplOvertime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OvertimeMpls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TotalOvertimeHours = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MPLConverted = table.Column<int>(type: "int", nullable: false),
                    ResidualOvertimeHours = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CutoffStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CutoffEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConversionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OvertimeMpls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OvertimeMpls_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OvertimeMpls_UserId",
                table: "OvertimeMpls",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OvertimeMpls");
        }
    }
}
