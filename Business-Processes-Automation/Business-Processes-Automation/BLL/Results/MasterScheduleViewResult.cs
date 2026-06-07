
namespace Business_Processes_Automation.BLL.Results;

public sealed class MasterScheduleViewResult
{
    public IReadOnlyList<string> MessageParts { get; init; } = [];

    public IReadOnlyList<ScheduleAppointmentListItem> Appointments { get; init; } = [];
}
