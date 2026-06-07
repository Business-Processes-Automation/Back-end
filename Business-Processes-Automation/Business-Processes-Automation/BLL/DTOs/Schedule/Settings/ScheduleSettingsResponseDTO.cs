namespace Business_Processes_Automation.BLL.DTOs.Schedule.Settings;

public class ScheduleSettingsResponseDTO
{
    public int BufferBetweenClientsMinutes { get; set; }

    public int FreeSlotIntervalMinutes { get; set; }

    public int MinBookingNoticeMinutes { get; set; }

    public int MaxBookingDaysAhead { get; set; }

    public int MaxRescheduleCount { get; set; }

    public int CancellationPolicyHours { get; set; }
}
