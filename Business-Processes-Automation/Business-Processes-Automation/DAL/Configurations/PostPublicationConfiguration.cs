using Business_Processes_Automation.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business_Processes_Automation.DAL.Configurations;

public class PostPublicationConfiguration : IEntityTypeConfiguration<PostPublication>
{
    public void Configure(EntityTypeBuilder<PostPublication> builder)
    {
        builder.ConfigureBaseEntity();
        builder.ToTable("PostPublications");

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasOne(x => x.Post)
            .WithMany(x => x.PostPublications)
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Platform)
            .WithMany(x => x.PostPublications)
            .HasForeignKey(x => x.PlatformId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.PostId, x.PlatformId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_PostPublications_PublishedAt_WhenPublished",
            "[Status] <> 'Published' OR [PublishedAt] IS NOT NULL"));
    }
}
