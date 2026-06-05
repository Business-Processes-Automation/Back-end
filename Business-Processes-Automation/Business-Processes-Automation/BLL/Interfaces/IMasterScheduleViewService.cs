using Business_Processes_Automation.BLL.DTOs.Schedule;
using Business_Processes_Automation.BLL.Enums;

using Business_Processes_Automation.BLL.Results;

namespace Business_Processes_Automation.BLL.Services;

public interface IMasterScheduleViewService
{
    Task<MasterScheduleViewResult> BuildViewAsync(
        int masterId,
        ScheduleViewPeriod period,
        CancellationToken cancellationToken = default);

    Task<string?> BuildAppointmentDetailsAsync(
        int masterId,
        int appointmentId,
        CancellationToken cancellationToken = default);
}
