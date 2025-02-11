using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendanceTracker1.Migrations
{
    /// <inheritdoc />
    public partial class AddedOfficeStartandEndTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "OfficeEndTime",
                table: "OvertimeConfigs",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "OfficeStartTime",
                table: "OvertimeConfigs",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OfficeEndTime",
                table: "OvertimeConfigs");

            migrationBuilder.DropColumn(
                name: "OfficeStartTime",
                table: "OvertimeConfigs");
        }
    }
}
