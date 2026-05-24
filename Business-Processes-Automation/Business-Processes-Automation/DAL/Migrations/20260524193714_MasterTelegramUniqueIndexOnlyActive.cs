using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Business_Processes_Automation.DAL.Migrations;

public partial class MasterTelegramUniqueIndexOnlyActive : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_MasterTelegrams_BotStartParameter",
            table: "MasterTelegrams");

        migrationBuilder.DropIndex(
            name: "IX_MasterTelegrams_MasterId",
            table: "MasterTelegrams");

        migrationBuilder.DropIndex(
            name: "IX_MasterTelegrams_TelegramUserId",
            table: "MasterTelegrams");

        migrationBuilder.CreateIndex(
            name: "IX_MasterTelegrams_BotStartParameter",
            table: "MasterTelegrams",
            column: "BotStartParameter",
            unique: true,
            filter: "[IsDeleted] = 0");

        migrationBuilder.CreateIndex(
            name: "IX_MasterTelegrams_MasterId",
            table: "MasterTelegrams",
            column: "MasterId",
            unique: true,
            filter: "[IsDeleted] = 0");

        migrationBuilder.CreateIndex(
            name: "IX_MasterTelegrams_TelegramUserId",
            table: "MasterTelegrams",
            column: "TelegramUserId",
            unique: true,
            filter: "[IsDeleted] = 0");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_MasterTelegrams_BotStartParameter",
            table: "MasterTelegrams");

        migrationBuilder.DropIndex(
            name: "IX_MasterTelegrams_MasterId",
            table: "MasterTelegrams");

        migrationBuilder.DropIndex(
            name: "IX_MasterTelegrams_TelegramUserId",
            table: "MasterTelegrams");

        migrationBuilder.CreateIndex(
            name: "IX_MasterTelegrams_BotStartParameter",
            table: "MasterTelegrams",
            column: "BotStartParameter",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_MasterTelegrams_MasterId",
            table: "MasterTelegrams",
            column: "MasterId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_MasterTelegrams_TelegramUserId",
            table: "MasterTelegrams",
            column: "TelegramUserId",
            unique: true);
    }
}
