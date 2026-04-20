namespace Business_Processes_Automation.DAL.Entities;

public class Expense : BaseEntity
{
    public int MasterId { get; set; }
    public int ExpenseCategoryId { get; set; }
    public decimal Amount { get; set; }
    public DateTime DateOfExpense { get; set; }
    public string? Description { get; set; }

    // Navigation property
    public Master Master { get; set; } = null!;
    public ExpenseCategory ExpenseCategory { get; set; } = null!;
}
