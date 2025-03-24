using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendanceTracker1.Migrations
{
    /// <inheritdoc />
    public partial class AddedUpdatedLeaveDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Leaves",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Leaves");
        }
    }
}
