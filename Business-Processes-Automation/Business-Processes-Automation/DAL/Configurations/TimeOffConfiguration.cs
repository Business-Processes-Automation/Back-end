using Business_Processes_Automation.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business_Processes_Automation.DAL.Configurations;

public class TimeOffConfiguration : IEntityTypeConfiguration<TimeOff>
{
    public void Configure(EntityTypeBuilder<TimeOff> builder)
    {
        builder.ConfigureBaseEntity();
        builder.ToTable("TimeOffs");

        builder.HasOne(x => x.Master)
            .WithMany(x => x.TimeOffs)
            .HasForeignKey(x => x.MasterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.MasterId, x.StartDateTime, x.EndDateTime });

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_TimeOffs_DateRange",
            "[EndDateTime] > [StartDateTime]"));
    }
}
