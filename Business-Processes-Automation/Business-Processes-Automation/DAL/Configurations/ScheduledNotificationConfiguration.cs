using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business_Processes_Automation.DAL.Configurations;

public class ScheduledNotificationConfiguration : IEntityTypeConfiguration<ScheduledNotification>
{
    public void Configure(EntityTypeBuilder<ScheduledNotification> builder)
    {
        builder.ConfigureBaseEntity();
        builder.ToTable("ScheduledNotifications");

        builder.Property(x => x.Kind)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(x => x.MessageText)
            .HasMaxLength(4096)
            .IsRequired();

        builder.Property(x => x.LastError)
            .HasMaxLength(2000);

        builder.Property(x => x.ScheduledAtUtc).IsRequired();

        builder.HasOne(x => x.Appointment)
            .WithMany(x => x.ScheduledNotifications)
            .HasForeignKey(x => x.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.Status, x.ScheduledAtUtc });

        builder.HasIndex(x => new { x.AppointmentId, x.Status });

        builder.HasIndex(x => new { x.AppointmentId, x.Kind })
            .IsUnique()
            .HasFilter("[Status] = N'Pending' AND [IsDeleted] = 0");
    }
}
