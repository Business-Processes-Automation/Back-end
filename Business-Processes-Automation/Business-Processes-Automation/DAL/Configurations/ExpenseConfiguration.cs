using Business_Processes_Automation.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business_Processes_Automation.DAL.Configurations;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ConfigureBaseEntity();
        builder.ToTable("Expenses");

        builder.Property(x => x.Amount).ConfigureMoney();
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.HasOne(x => x.Master)
            .WithMany(x => x.Expenses)
            .HasForeignKey(x => x.MasterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ExpenseCategory)
            .WithMany(x => x.Expenses)
            .HasForeignKey(x => x.ExpenseCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.MasterId, x.DateOfExpense });

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Expenses_Amount",
            "[Amount] > 0"));
    }
}
