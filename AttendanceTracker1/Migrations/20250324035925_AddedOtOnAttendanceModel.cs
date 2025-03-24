using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendanceTracker1.Migrations
{
    /// <inheritdoc />
    public partial class AddedOtOnAttendanceModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "OvertimeDuration",
                table: "Attendances",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OvertimeDuration",
                table: "Attendances");
        }
    }
}
