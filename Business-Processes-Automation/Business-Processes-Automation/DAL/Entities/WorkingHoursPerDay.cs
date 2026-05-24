using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.DAL.Entities;

public class WorkingHoursPerDay : BaseEntity
{
    public int MasterId { get; set; }

    public Weekday DayOfWeek { get; set; }

    public TimeOnly WorkStartTime { get; set; }
    public TimeOnly WorkEndTime { get; set; }

    public Master Master { get; set; } = null!;
}
