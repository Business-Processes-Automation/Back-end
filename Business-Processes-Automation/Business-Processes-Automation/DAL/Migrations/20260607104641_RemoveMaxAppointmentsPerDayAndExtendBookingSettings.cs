using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Business_Processes_Automation.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMaxAppointmentsPerDayAndExtendBookingSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_MasterAppointmentSettings_CancellationPolicyHours",
                table: "MasterAppointmentSettings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MasterAppointmentSettings_MaxAppointmentsPerDay",
                table: "MasterAppointmentSettings");

            migrationBuilder.DropColumn(
                name: "MaxAppointmentsPerDay",
                table: "MasterAppointmentSettings");

            migrationBuilder.AlterColumn<int>(
                name: "MaxRescheduleCount",
                table: "MasterAppointmentSettings",
                type: "int",
                nullable: false,
                defaultValue: 2,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AddCheckConstraint(
                name: "CK_MasterAppointmentSettings_CancellationPolicyHours",
                table: "MasterAppointmentSettings",
                sql: "[CancellationPolicyHours] >= 0 AND [CancellationPolicyHours] <= 168");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_MasterAppointmentSettings_CancellationPolicyHours",
                table: "MasterAppointmentSettings");

            migrationBuilder.AlterColumn<int>(
                name: "MaxRescheduleCount",
                table: "MasterAppointmentSettings",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "MaxAppointmentsPerDay",
                table: "MasterAppointmentSettings",
                type: "int",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_MasterAppointmentSettings_CancellationPolicyHours",
                table: "MasterAppointmentSettings",
                sql: "[CancellationPolicyHours] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MasterAppointmentSettings_MaxAppointmentsPerDay",
                table: "MasterAppointmentSettings",
                sql: "[MaxAppointmentsPerDay] IS NULL OR ([MaxAppointmentsPerDay] >= 1 AND [MaxAppointmentsPerDay] <= 100)");
        }
    }
}
