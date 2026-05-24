using Business_Processes_Automation.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business_Processes_Automation.DAL.Configurations;

public class MasterTelegramConfiguration : IEntityTypeConfiguration<MasterTelegram>
{
    public void Configure(EntityTypeBuilder<MasterTelegram> builder)
    {
        builder.ConfigureBaseEntity();
        builder.ToTable("MasterTelegrams");

        builder.Property(x => x.TelegramUsername).HasMaxLength(64).IsRequired();
        builder.Property(x => x.BotStartParameter).HasMaxLength(64).IsRequired();

        builder.HasOne(x => x.Master)
            .WithOne(x => x.MasterTelegram)
            .HasForeignKey<MasterTelegram>(x => x.MasterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.MasterId).IsUnique();
        builder.HasIndex(x => x.TelegramUserId).IsUnique();
        builder.HasIndex(x => x.BotStartParameter).IsUnique();
    }
}
