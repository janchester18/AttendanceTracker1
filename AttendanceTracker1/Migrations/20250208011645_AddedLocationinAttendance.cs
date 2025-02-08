using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendanceTracker1.Migrations
{
    /// <inheritdoc />
    public partial class AddedLocationinAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ClockInLatitude",
                table: "Attendances",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ClockInLongitude",
                table: "Attendances",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ClockOutLatitude",
                table: "Attendances",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ClockOutLongitude",
                table: "Attendances",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClockInLatitude",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "ClockInLongitude",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "ClockOutLatitude",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "ClockOutLongitude",
                table: "Attendances");
        }
    }
}
