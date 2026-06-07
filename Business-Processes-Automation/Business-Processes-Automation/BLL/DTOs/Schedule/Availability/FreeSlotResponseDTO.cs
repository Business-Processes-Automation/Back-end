namespace Business_Processes_Automation.BLL.DTOs.Schedule.Availability;

public class FreeSlotResponseDTO
{
    public DateOnly Date { get; set; }

    public DateTime StartLocal { get; set; }

    public DateTime EndLocal { get; set; }
}
