using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendanceTracker1.Migrations
{
    /// <inheritdoc />
    public partial class AddedMpl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccumulatedNightDifferential",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "AccumulatedOvertime",
                table: "Users",
                newName: "Mpl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Mpl",
                table: "Users",
                newName: "AccumulatedOvertime");

            migrationBuilder.AddColumn<double>(
                name: "AccumulatedNightDifferential",
                table: "Users",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
