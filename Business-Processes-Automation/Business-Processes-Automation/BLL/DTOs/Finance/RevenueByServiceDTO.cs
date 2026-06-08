namespace Business_Processes_Automation.BLL.DTOs.Finance;

public class RevenueByServiceDTO
{
    public int ServiceId { get; set; }

    public string ServiceName { get; set; } = null!;

    public int Count { get; set; }

    public decimal Total { get; set; }
}
