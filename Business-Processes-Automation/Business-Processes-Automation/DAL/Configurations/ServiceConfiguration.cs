using Business_Processes_Automation.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business_Processes_Automation.DAL.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ConfigureBaseEntity();
        builder.ToTable("Services");

        builder.Property(x => x.ServiceName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Price).ConfigureMoney();
        builder.Property(x => x.Prepayment).ConfigureMoney();

        builder.HasOne(x => x.Master)
            .WithMany(x => x.Services)
            .HasForeignKey(x => x.MasterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.MasterId, x.ServiceName })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Services_DurationInMinutes",
            "[DurationInMinutes] > 0"));

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Services_Price",
            "[Price] >= 0"));

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Services_Prepayment",
            "[Prepayment] >= 0 AND [Prepayment] <= [Price]"));

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Services_PreparationMinutes",
            "[PreparationBeforeInMinutes] >= 0 AND [PreparationAfterInMinutes] >= 0"));

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Services_TotalOccupiedMinutes",
            "[TotalOccupiedMinutes] > 0"));
    }
}
