using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.Services;

public interface IMasterAvailabilityService
{
    Task<TimeZoneInfo> GetMasterTimeZoneAsync(int masterId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkingHoursPerDay>> GetWorkingHoursForMasterAsync(
        int masterId,
        CancellationToken cancellationToken = default);

    Task<MasterAppointmentSetting?> GetSettingsForMasterAsync(
        int masterId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TimeOff>> GetTimeOffsAsync(
        int masterId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Appointment>> GetAppointmentsAsync(
        int masterId,
        DateTime fromUtc,
        DateTime toUtc,
        IReadOnlyCollection<AppointmentStatus>? statuses = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FreeSlot>> GetFreeSlotsForPeriodAsync(
        int masterId,
        DateOnly rangeStart,
        DateOnly rangeEnd,
        int occupiedMinutes,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TimeOnly>> GetFreeSlotsAsync(
        int masterId,
        DateOnly localDate,
        int occupiedMinutes,
        CancellationToken cancellationToken = default);
}
