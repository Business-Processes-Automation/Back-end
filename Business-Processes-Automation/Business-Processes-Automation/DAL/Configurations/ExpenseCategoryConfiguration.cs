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

        var seedCreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData(
            new ExpenseCategory
            {
                Id = 1,
                NameOfExpense = "Матеріали",
                Description = "Косметика, інструменти, расходники",
                CreatedAt = seedCreatedAt,
                IsDeleted = false
            },
            new ExpenseCategory
            {
                Id = 2,
                NameOfExpense = "Оренда",
                Description = "Оренда приміщення або робочого місця",
                CreatedAt = seedCreatedAt,
                IsDeleted = false
            },
            new ExpenseCategory
            {
                Id = 3,
                NameOfExpense = "Реклама",
                Description = "Реклама в соцмережах, оголошення",
                CreatedAt = seedCreatedAt,
                IsDeleted = false
            },
            new ExpenseCategory
            {
                Id = 4,
                NameOfExpense = "Транспорт",
                Description = "Проїзд, доставка, паливо",
                CreatedAt = seedCreatedAt,
                IsDeleted = false
            },
            new ExpenseCategory
            {
                Id = 5,
                NameOfExpense = "Інше",
                CreatedAt = seedCreatedAt,
                IsDeleted = false
            });
    }
}
