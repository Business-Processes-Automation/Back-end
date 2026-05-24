using Business_Processes_Automation.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business_Processes_Automation.DAL.Configurations;

public class NotificationChannelConfiguration : IEntityTypeConfiguration<NotificationChannel>
{
    public void Configure(EntityTypeBuilder<NotificationChannel> builder)
    {
        builder.ConfigureBaseEntity();
        builder.ToTable("NotificationChannels");

        builder.Property(x => x.NotificationName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NotificationDescription).HasMaxLength(500);

        builder.HasIndex(x => x.NotificationName)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
