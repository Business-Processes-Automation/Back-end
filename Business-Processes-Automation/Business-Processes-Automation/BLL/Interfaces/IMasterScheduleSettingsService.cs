using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.Services;

public interface IMasterScheduleSettingsService
{
    Task<IReadOnlyList<WorkingHoursPerDay>> GetWorkingHoursAsync(
        int masterId,
        CancellationToken cancellationToken = default);

    Task<MasterAppointmentSetting?> GetSettingAsync(
        int masterId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TimeOff>> GetTimeOffsInRangeAsync(
        int masterId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default);

    Task<ScheduleSettingsResult> UpsertWorkingHoursAsync(
        int masterId,
        Weekday day,
        TimeOnly workStartTime,
        TimeOnly workEndTime,
        CancellationToken cancellationToken = default);

    Task<ScheduleSettingsResult> DeleteWorkingHoursForDayAsync(
        int masterId,
        Weekday day,
        CancellationToken cancellationToken = default);

    Task<ScheduleSettingsResult> UpdateBufferMinutesAsync(
        int masterId,
        int bufferMinutes,
        CancellationToken cancellationToken = default);

    Task<ScheduleSettingsResult> CreateTimeOffAsync(
        int masterId,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteTimeOffAsync(int timeOffId, CancellationToken cancellationToken = default);
}
