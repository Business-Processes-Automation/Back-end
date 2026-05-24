using Business_Processes_Automation.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business_Processes_Automation.DAL.Configurations;

public class ExpenseCategoryConfiguration : IEntityTypeConfiguration<ExpenseCategory>
{
    public void Configure(EntityTypeBuilder<ExpenseCategory> builder)
    {
        builder.ConfigureBaseEntity();
        builder.ToTable("ExpenseCategories");

        builder.Property(x => x.NameOfExpense).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.HasIndex(x => x.NameOfExpense)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
