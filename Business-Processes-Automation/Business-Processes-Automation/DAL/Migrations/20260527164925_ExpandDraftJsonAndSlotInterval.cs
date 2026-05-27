using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Business_Processes_Automation.DAL.Migrations;

/// <inheritdoc />
public partial class ExpandDraftJsonAndSlotInterval : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "DraftJson",
            table: "TelegramUserSessions",
            type: "nvarchar(max)",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(1000)",
            oldMaxLength: 1000,
            oldNullable: true);

        migrationBuilder.AddColumn<int>(
            name: "FreeSlotIntervalMinutes",
            table: "MasterAppointmentSettings",
            type: "int",
            nullable: false,
            defaultValue: 15);

        migrationBuilder.AddCheckConstraint(
            name: "CK_MasterAppointmentSettings_FreeSlotIntervalMinutes",
            table: "MasterAppointmentSettings",
            sql: "[FreeSlotIntervalMinutes] >= 5 AND [FreeSlotIntervalMinutes] <= 120 AND [FreeSlotIntervalMinutes] % 5 = 0");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(
            name: "CK_MasterAppointmentSettings_FreeSlotIntervalMinutes",
            table: "MasterAppointmentSettings");

        migrationBuilder.DropColumn(
            name: "FreeSlotIntervalMinutes",
            table: "MasterAppointmentSettings");

        migrationBuilder.AlterColumn<string>(
            name: "DraftJson",
            table: "TelegramUserSessions",
            type: "nvarchar(1000)",
            maxLength: 1000,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)",
            oldNullable: true);
    }
}
