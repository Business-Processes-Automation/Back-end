using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Business_Processes_Automation.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddMasterTelegramLinkCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TelegramLinkCode",
                table: "Masters",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TelegramLinkCodeExpiresAtUtc",
                table: "Masters",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Masters_TelegramLinkCode",
                table: "Masters",
                column: "TelegramLinkCode",
                unique: true,
                filter: "[TelegramLinkCode] IS NOT NULL AND [IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Masters_TelegramLinkCode",
                table: "Masters");

            migrationBuilder.DropColumn(
                name: "TelegramLinkCode",
                table: "Masters");

            migrationBuilder.DropColumn(
                name: "TelegramLinkCodeExpiresAtUtc",
                table: "Masters");
        }
    }
}
