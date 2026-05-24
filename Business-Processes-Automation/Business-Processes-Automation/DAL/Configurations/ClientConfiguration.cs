using Business_Processes_Automation.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business_Processes_Automation.DAL.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ConfigureBaseEntity();
        builder.ToTable("Clients");

        builder.Property(x => x.ClientName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ClientPhone).HasMaxLength(20).IsRequired();
        builder.Property(x => x.ClientEmail).HasMaxLength(256);
        builder.Property(x => x.DepositBalance).ConfigureMoney().HasDefaultValue(0m);

        builder.HasIndex(x => x.ClientPhone)
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(x => x.ClientTelegramId)
            .IsUnique()
            .HasFilter("[ClientTelegramId] IS NOT NULL AND [IsDeleted] = 0");

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Clients_DepositBalance",
            "[DepositBalance] >= 0"));
    }
}
