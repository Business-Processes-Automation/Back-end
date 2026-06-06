using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Business_Processes_Automation.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceTotalOccupiedMinutes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalOccupiedMinutes",
                table: "Services",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                UPDATE Services
                SET TotalOccupiedMinutes = PreparationBeforeInMinutes + DurationInMinutes + PreparationAfterInMinutes
                WHERE TotalOccupiedMinutes = 0
                """);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Services_TotalOccupiedMinutes",
                table: "Services",
                sql: "[TotalOccupiedMinutes] > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Services_TotalOccupiedMinutes",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "TotalOccupiedMinutes",
                table: "Services");
        }
    }
}
