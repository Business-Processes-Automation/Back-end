using Business_Processes_Automation.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business_Processes_Automation.DAL.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ConfigureBaseEntity();
        builder.ToTable("Appointments");

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.PriceAtBooking).ConfigureMoney();
        builder.Property(x => x.PrepaymentAmount).ConfigureMoney();
        builder.Property(x => x.RescheduleCount).HasDefaultValue(0);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder.HasOne(x => x.Client)
            .WithMany(x => x.Appointments)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Service)
            .WithMany(x => x.Appointments)
            .HasForeignKey(x => x.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.ServiceId, x.StartDateTime });
        builder.HasIndex(x => new { x.ClientId, x.StartDateTime });
        builder.HasIndex(x => x.Status);

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Appointments_DateRange",
            "[EndDateTime] > [StartDateTime]"));

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Appointments_PriceAtBooking",
            "[PriceAtBooking] >= 0"));

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Appointments_PrepaymentAmount",
            "[PrepaymentAmount] >= 0 AND [PrepaymentAmount] <= [PriceAtBooking]"));

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Appointments_RescheduleCount",
            "[RescheduleCount] >= 0 AND [RescheduleCount] <= 10"));
    }
}
