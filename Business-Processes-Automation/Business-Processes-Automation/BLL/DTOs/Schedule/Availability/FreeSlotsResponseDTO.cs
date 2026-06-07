namespace Business_Processes_Automation.BLL.DTOs.Schedule.Availability;

public class FreeSlotsResponseDTO
{
    public string TimeZone { get; set; } = null!;

    public DateOnly From { get; set; }

    public DateOnly To { get; set; }

    public int ServiceId { get; set; }

    public string ServiceName { get; set; } = null!;

    public int TotalOccupiedMinutes { get; set; }

    public IReadOnlyList<FreeSlotResponseDTO> Slots { get; set; } = [];
}
