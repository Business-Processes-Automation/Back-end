using Business_Processes_Automation.BLL.DTOs.Schedule;

namespace Business_Processes_Automation.BLL.SessionDrafts;

public class ScheduleViewDraft
{
    public List<ScheduleAppointmentListItem> Appointments { get; set; } = [];
}
