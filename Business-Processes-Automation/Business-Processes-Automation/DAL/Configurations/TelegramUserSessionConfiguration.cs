using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business_Processes_Automation.DAL.Configurations;

public class TelegramUserSessionConfiguration : IEntityTypeConfiguration<TelegramUserSession>
{
    public void Configure(EntityTypeBuilder<TelegramUserSession> builder)
    {
        builder.ConfigureBaseEntity();
        builder.ToTable("TelegramUserSessions");

        builder.Property(x => x.Role)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(x => x.CurrentStep)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.DraftJson)
            .HasMaxLength(1000);

        builder.HasOne(x => x.Master)
            .WithMany()
            .HasForeignKey(x => x.MasterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.TelegramUserId)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(x => x.ChatId);
    }
}
