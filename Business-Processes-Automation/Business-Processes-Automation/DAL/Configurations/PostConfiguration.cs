using Business_Processes_Automation.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business_Processes_Automation.DAL.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ConfigureBaseEntity();
        builder.ToTable("Posts");

        builder.Property(x => x.Text).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.ImageUrl).HasMaxLength(2048);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasOne(x => x.Master)
            .WithMany(x => x.Posts)
            .HasForeignKey(x => x.MasterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.MasterId, x.Status });
        builder.HasIndex(x => x.ScheduledAt);

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Posts_ScheduledAt_WhenScheduled",
            "[Status] <> 'Scheduled' OR [ScheduledAt] IS NOT NULL"));

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Posts_PublishedAt_WhenPublished",
            "[Status] <> 'Published' OR [PublishedAt] IS NOT NULL"));
    }
}
