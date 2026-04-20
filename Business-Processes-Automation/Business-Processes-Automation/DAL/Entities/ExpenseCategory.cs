namespace Business_Processes_Automation.DAL.Entities;

public class ExpenseCategory : BaseEntity
{
    public string NameOfExpenses { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation property
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
