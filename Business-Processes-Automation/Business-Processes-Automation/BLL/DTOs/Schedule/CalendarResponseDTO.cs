namespace Business_Processes_Automation.BLL.DTOs.Schedule;

public class CalendarResponseDTO
{
    public string TimeZone { get; set; } = null!;

    public DateOnly From { get; set; }

    public DateOnly To { get; set; }

    public IReadOnlyList<CalendarDayResponseDTO> Days { get; set; } = [];
}
