using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.SessionDrafts;

public class WorkHoursEditDraft
{
    public Weekday? Day { get; set; }

    public TimeOnly? WorkStartTime { get; set; }
}
