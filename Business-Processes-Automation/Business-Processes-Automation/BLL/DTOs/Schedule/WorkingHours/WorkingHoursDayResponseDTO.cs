using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.DTOs.Schedule.WorkingHours;

public class WorkingHoursDayResponseDTO
{
    public Weekday DayOfWeek { get; set; }

    public bool IsWorking { get; set; }

    public string? WorkStartTime { get; set; }

    public string? WorkEndTime { get; set; }
}
