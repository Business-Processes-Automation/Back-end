namespace Business_Processes_Automation.BLL.DTOs.Schedule;

public class ScheduleSettingsResponseDTO
{
    public int BufferBetweenClientsMinutes { get; set; }

    public int FreeSlotIntervalMinutes { get; set; }

    public int MinBookingNoticeMinutes { get; set; }
}
