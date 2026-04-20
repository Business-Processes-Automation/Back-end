namespace Business_Processes_Automation.DAL.Entities;

public class WorkingHoursPerDay : BaseEntity
{
    public int MasterId { get; set; }
    public int DayOfWeek { get; set; }
    public TimeOnly WorkStartTime { get; set; }
    public TimeOnly WorkEndTime { get; set; }

    // Navigation property
    public Master Master { get; set; } = null!;
}
