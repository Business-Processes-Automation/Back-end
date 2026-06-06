using Business_Processes_Automation.BLL.DTOs.Schedule;

namespace Business_Processes_Automation.BLL.Interfaces;

public interface IMasterScheduleApiService
{
    Task<CalendarResponseDTO> GetCalendarAsync(
        int masterId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);

    Task<AppointmentDetailsResponseDTO?> GetAppointmentDetailsAsync(
        int masterId,
        int appointmentId,
        CancellationToken cancellationToken = default);
}
