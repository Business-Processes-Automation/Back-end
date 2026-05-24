using Business_Processes_Automation.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business_Processes_Automation.DAL.Configurations;

public class SocialAccountConfiguration : IEntityTypeConfiguration<SocialAccount>
{
    public void Configure(EntityTypeBuilder<SocialAccount> builder)
    {
        builder.ConfigureBaseEntity();
        builder.ToTable("SocialAccounts");

        builder.Property(x => x.AccessToken).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.RefreshToken).HasMaxLength(2000);
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.HasOne(x => x.Master)
            .WithMany(x => x.SocialAccounts)
            .HasForeignKey(x => x.MasterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Platform)
            .WithMany(x => x.SocialAccounts)
            .HasForeignKey(x => x.PlatformId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.MasterId, x.PlatformId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
