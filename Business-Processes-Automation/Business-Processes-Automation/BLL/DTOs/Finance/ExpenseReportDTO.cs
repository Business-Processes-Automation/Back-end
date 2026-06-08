namespace Business_Processes_Automation.BLL.DTOs.Finance;

public class ExpenseReportDTO
{
    public DateOnly From { get; set; }

    public DateOnly To { get; set; }

    public decimal TotalExpenses { get; set; }

    public int ExpensesCount { get; set; }

    public IReadOnlyList<ExpenseResponseDTO> Items { get; set; } = [];

    public IReadOnlyList<ExpensesByCategoryDTO> ByCategory { get; set; } = [];
}
