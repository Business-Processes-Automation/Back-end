namespace Business_Processes_Automation.BLL.DTOs.Finance;

public class ExpenseResponseDTO
{
    public int Id { get; set; }

    public int ExpenseCategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateOnly DateOfExpense { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
