using Business_Processes_Automation.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business_Processes_Automation.DAL.Configurations;

public class PlatformConfiguration : IEntityTypeConfiguration<Platform>
{
    public void Configure(EntityTypeBuilder<Platform> builder)
    {
        builder.ConfigureBaseEntity();
        builder.ToTable("Platforms");

        builder.Property(x => x.PlatformName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.HasIndex(x => x.PlatformName)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
