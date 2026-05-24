using Business_Processes_Automation.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business_Processes_Automation.DAL.Configurations;

public class MasterNotificationPreferenceConfiguration : IEntityTypeConfiguration<MasterNotificationPreference>
{
    public void Configure(EntityTypeBuilder<MasterNotificationPreference> builder)
    {
        builder.ConfigureBaseEntity();
        builder.ToTable("MasterNotificationPreferences");

        builder.Property(x => x.IsEnabled).HasDefaultValue(true);

        builder.HasOne(x => x.Master)
            .WithMany(x => x.NotificationPreferences)
            .HasForeignKey(x => x.MasterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.NotificationChannel)
            .WithMany(x => x.MasterNotificationPreferences)
            .HasForeignKey(x => x.NotificationChannelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.NotificationType)
            .WithMany(x => x.MasterNotificationPreferences)
            .HasForeignKey(x => x.NotificationTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.MasterId, x.NotificationChannelId, x.NotificationTypeId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
