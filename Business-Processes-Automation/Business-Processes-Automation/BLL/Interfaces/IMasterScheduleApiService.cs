using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.Interfaces;

public interface IMasterScheduleApiService
{
    Task<CalendarResponseDTO> GetCalendarAsync(
        int masterId,
        DateOnly from,
        DateOnly to,
        bool includeCancelled = false,
        AppointmentStatus? status = null,
        int? serviceId = null,
        CancellationToken cancellationToken = default);

    Task<FreeSlotsResponseDTO> GetFreeSlotsAsync(
        int masterId,
        DateOnly from,
        DateOnly to,
        int serviceId,
        CancellationToken cancellationToken = default);

    Task<AppointmentDetailsResponseDTO?> GetAppointmentDetailsAsync(
        int masterId,
        int appointmentId,
        CancellationToken cancellationToken = default);
}
