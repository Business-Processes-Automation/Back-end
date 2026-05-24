using Business_Processes_Automation.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business_Processes_Automation.DAL.Configurations;

public class ClientNotificationPreferenceConfiguration : IEntityTypeConfiguration<ClientNotificationPreference>
{
    public void Configure(EntityTypeBuilder<ClientNotificationPreference> builder)
    {
        builder.ConfigureBaseEntity();
        builder.ToTable("ClientNotificationPreferences");

        builder.Property(x => x.IsEnabled).HasDefaultValue(true);

        builder.HasOne(x => x.Client)
            .WithMany(x => x.NotificationPreferences)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.NotificationChannel)
            .WithMany(x => x.ClientNotificationPreferences)
            .HasForeignKey(x => x.NotificationChannelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.NotificationType)
            .WithMany(x => x.ClientNotificationPreferences)
            .HasForeignKey(x => x.NotificationTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.ClientId, x.NotificationChannelId, x.NotificationTypeId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
