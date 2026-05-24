using System.ComponentModel.DataAnnotations;

namespace Business_Processes_Automation.DAL.Entities;

public class Expense : BaseEntity
{
    public int MasterId { get; set; }
    public int ExpenseCategoryId { get; set; }

    [Range(typeof(decimal), "0.01", "999999.99")]
    public decimal Amount { get; set; }

    public DateTime DateOfExpense { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public Master Master { get; set; } = null!;
    public ExpenseCategory ExpenseCategory { get; set; } = null!;
}
