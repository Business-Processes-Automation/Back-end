using Business_Processes_Automation.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business_Processes_Automation.DAL.Configurations;

public class MasterAppointmentSettingConfiguration : IEntityTypeConfiguration<MasterAppointmentSetting>
{
    public void Configure(EntityTypeBuilder<MasterAppointmentSetting> builder)
    {
        builder.ConfigureBaseEntity();
        builder.ToTable("MasterAppointmentSettings");

        builder.HasOne(x => x.Master)
            .WithOne(x => x.AppointmentSetting)
            .HasForeignKey<MasterAppointmentSetting>(x => x.MasterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.MasterId).IsUnique();

        builder.Property(x => x.BufferBetweenClientsMinutes).HasDefaultValue(0);
        builder.Property(x => x.FreeSlotIntervalMinutes).HasDefaultValue(15);
        builder.Property(x => x.MaxRescheduleCount).HasDefaultValue(2);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_MasterAppointmentSettings_MinBookingNoticeMinutes",
                "[MinBookingNoticeMinutes] >= 0");
            t.HasCheckConstraint(
                "CK_MasterAppointmentSettings_MaxBookingDaysAhead",
                "[MaxBookingDaysAhead] > 0 AND [MaxBookingDaysAhead] <= 365");
            t.HasCheckConstraint(
                "CK_MasterAppointmentSettings_CancellationPolicyHours",
                "[CancellationPolicyHours] >= 0 AND [CancellationPolicyHours] <= 168");
            t.HasCheckConstraint(
                "CK_MasterAppointmentSettings_BufferBetweenClientsMinutes",
                "[BufferBetweenClientsMinutes] >= 0 AND [BufferBetweenClientsMinutes] <= 480");
            t.HasCheckConstraint(
                "CK_MasterAppointmentSettings_FreeSlotIntervalMinutes",
                "[FreeSlotIntervalMinutes] >= 5 AND [FreeSlotIntervalMinutes] <= 120 AND [FreeSlotIntervalMinutes] % 5 = 0");
            t.HasCheckConstraint(
                "CK_MasterAppointmentSettings_MaxRescheduleCount",
                "[MaxRescheduleCount] >= 0 AND [MaxRescheduleCount] <= 10");
        });
    }
}
