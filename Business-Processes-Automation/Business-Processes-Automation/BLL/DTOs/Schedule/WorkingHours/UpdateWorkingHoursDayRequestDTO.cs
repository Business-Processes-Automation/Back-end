namespace Business_Processes_Automation.BLL.DTOs.Schedule.WorkingHours;

public class UpdateWorkingHoursDayRequestDTO
{
    public bool IsWorking { get; set; }

    public string? WorkStartTime { get; set; }

    public string? WorkEndTime { get; set; }
}
