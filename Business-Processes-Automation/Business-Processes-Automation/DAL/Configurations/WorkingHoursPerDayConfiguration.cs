using Business_Processes_Automation.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business_Processes_Automation.DAL.Configurations;

public class WorkingHoursPerDayConfiguration : IEntityTypeConfiguration<WorkingHoursPerDay>
{
    public void Configure(EntityTypeBuilder<WorkingHoursPerDay> builder)
    {
        builder.ConfigureBaseEntity();
        builder.ToTable("WorkingHoursPerDays");

        builder.Property(x => x.DayOfWeek)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.HasOne(x => x.Master)
            .WithMany(x => x.WorkingHours)
            .HasForeignKey(x => x.MasterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.MasterId, x.DayOfWeek })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_WorkingHoursPerDays_WorkTime",
            "[WorkEndTime] > [WorkStartTime]"));
    }
}
