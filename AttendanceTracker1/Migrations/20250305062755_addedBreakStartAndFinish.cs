using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendanceTracker1.Migrations
{
    /// <inheritdoc />
    public partial class addedBreakStartAndFinish : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BreaktimeMax",
                table: "OvertimeConfigs");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "BreakEndTime",
                table: "OvertimeConfigs",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "BreakStartTime",
                table: "OvertimeConfigs",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BreakEndTime",
                table: "OvertimeConfigs");

            migrationBuilder.DropColumn(
                name: "BreakStartTime",
                table: "OvertimeConfigs");

            migrationBuilder.AddColumn<double>(
                name: "BreaktimeMax",
                table: "OvertimeConfigs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
