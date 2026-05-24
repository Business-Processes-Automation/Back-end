using Business_Processes_Automation.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business_Processes_Automation.DAL.Configurations;

public class AIContentGenerationConfiguration : IEntityTypeConfiguration<AIContentGeneration>
{
    public void Configure(EntityTypeBuilder<AIContentGeneration> builder)
    {
        builder.ConfigureBaseEntity();
        builder.ToTable("AIContentGenerations");

        builder.Property(x => x.Prompt).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.GeneratedText).HasMaxLength(4000);
        builder.Property(x => x.GeneratedImageUrl).HasMaxLength(2048);

        builder.HasOne(x => x.Post)
            .WithMany(x => x.AiContentGenerations)
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.PostId);
    }
}
