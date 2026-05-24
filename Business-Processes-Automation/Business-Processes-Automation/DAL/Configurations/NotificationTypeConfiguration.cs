using Business_Processes_Automation.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business_Processes_Automation.DAL.Configurations;

public class NotificationTypeConfiguration : IEntityTypeConfiguration<NotificationType>
{
    public void Configure(EntityTypeBuilder<NotificationType> builder)
    {
        builder.ConfigureBaseEntity();
        builder.ToTable("NotificationTypes");

        builder.Property(x => x.TypeName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.TypeDescription).HasMaxLength(500);

        builder.HasIndex(x => x.TypeName)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
