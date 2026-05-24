using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Business_Processes_Automation.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingRulesAndClientDeposit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BufferBetweenClientsMinutes",
                table: "MasterAppointmentSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxAppointmentsPerDay",
                table: "MasterAppointmentSettings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxRescheduleCount",
                table: "MasterAppointmentSettings",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<decimal>(
                name: "DepositBalance",
                table: "Clients",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddCheckConstraint(
                name: "CK_MasterAppointmentSettings_BufferBetweenClientsMinutes",
                table: "MasterAppointmentSettings",
                sql: "[BufferBetweenClientsMinutes] >= 0 AND [BufferBetweenClientsMinutes] <= 480");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MasterAppointmentSettings_MaxAppointmentsPerDay",
                table: "MasterAppointmentSettings",
                sql: "[MaxAppointmentsPerDay] IS NULL OR ([MaxAppointmentsPerDay] >= 1 AND [MaxAppointmentsPerDay] <= 100)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MasterAppointmentSettings_MaxRescheduleCount",
                table: "MasterAppointmentSettings",
                sql: "[MaxRescheduleCount] >= 0 AND [MaxRescheduleCount] <= 10");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Clients_DepositBalance",
                table: "Clients",
                sql: "[DepositBalance] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_MasterAppointmentSettings_BufferBetweenClientsMinutes",
                table: "MasterAppointmentSettings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MasterAppointmentSettings_MaxAppointmentsPerDay",
                table: "MasterAppointmentSettings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MasterAppointmentSettings_MaxRescheduleCount",
                table: "MasterAppointmentSettings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Clients_DepositBalance",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "BufferBetweenClientsMinutes",
                table: "MasterAppointmentSettings");

            migrationBuilder.DropColumn(
                name: "MaxAppointmentsPerDay",
                table: "MasterAppointmentSettings");

            migrationBuilder.DropColumn(
                name: "MaxRescheduleCount",
                table: "MasterAppointmentSettings");

            migrationBuilder.DropColumn(
                name: "DepositBalance",
                table: "Clients");
        }
    }
}
