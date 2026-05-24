using Business_Processes_Automation.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business_Processes_Automation.DAL.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ConfigureBaseEntity();
        builder.ToTable("Payments");

        builder.Property(x => x.Amount).ConfigureMoney();

        builder.Property(x => x.PaymentMethod)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.PaymentStatus)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.PaymentType)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasOne(x => x.Appointment)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.AppointmentId);
        builder.HasIndex(x => x.PaymentStatus);

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Payments_Amount",
            "[Amount] > 0"));

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Payments_PaidAt_WhenPaid",
            "[PaymentStatus] <> 'Paid' OR [PaidAt] IS NOT NULL"));
    }
}
