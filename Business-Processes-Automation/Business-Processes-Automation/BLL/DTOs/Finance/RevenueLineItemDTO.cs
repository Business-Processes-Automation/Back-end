namespace Business_Processes_Automation.BLL.DTOs.Finance;

public class RevenueLineItemDTO
{
    public int AppointmentId { get; set; }

    public int ServiceId { get; set; }

    public string ServiceName { get; set; } = null!;

    public string ClientName { get; set; } = null!;

    public DateOnly VisitDate { get; set; }

    public DateTime StartLocal { get; set; }

    public decimal Amount { get; set; }
}
