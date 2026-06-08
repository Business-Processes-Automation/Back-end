namespace Business_Processes_Automation.BLL.DTOs.Finance;

public class RevenueReportDTO
{
    public DateOnly From { get; set; }

    public DateOnly To { get; set; }

    public string TimeZone { get; set; } = null!;

    public decimal TotalRevenue { get; set; }

    public int CompletedAppointmentsCount { get; set; }

    public IReadOnlyList<RevenueLineItemDTO> Items { get; set; } = [];

    public IReadOnlyList<RevenueByServiceDTO> ByService { get; set; } = [];
}
