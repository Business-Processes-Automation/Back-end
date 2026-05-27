namespace Business_Processes_Automation.BLL.DTOs.Schedule;

public sealed class MasterScheduleViewResult
{
    public IReadOnlyList<string> MessageParts { get; init; } = [];

    public IReadOnlyList<ScheduleAppointmentListItem> Appointments { get; init; } = [];
}
