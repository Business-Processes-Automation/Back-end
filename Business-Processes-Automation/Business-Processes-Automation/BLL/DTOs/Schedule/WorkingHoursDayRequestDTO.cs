using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.DTOs.Schedule;

public class WorkingHoursDayRequestDTO
{
    public Weekday DayOfWeek { get; set; }

    public bool IsWorking { get; set; }

    public string? WorkStartTime { get; set; }

    public string? WorkEndTime { get; set; }
}
