using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Business_Processes_Automation.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddMasterPasswordHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Masters",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Masters");
        }
    }
}
