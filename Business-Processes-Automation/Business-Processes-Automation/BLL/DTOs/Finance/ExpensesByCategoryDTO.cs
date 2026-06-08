namespace Business_Processes_Automation.BLL.DTOs.Finance;

public class ExpensesByCategoryDTO
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public int Count { get; set; }

    public decimal Total { get; set; }
}
