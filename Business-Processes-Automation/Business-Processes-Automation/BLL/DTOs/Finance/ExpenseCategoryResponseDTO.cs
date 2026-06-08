namespace Business_Processes_Automation.BLL.DTOs.Finance;

public class ExpenseCategoryResponseDTO
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }
}
