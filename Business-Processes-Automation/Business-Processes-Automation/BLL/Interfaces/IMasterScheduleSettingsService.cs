using Business_Processes_Automation.BLL.DTOs.Schedule;
using Business_Processes_Automation.BLL.Results;
using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.Services;

public interface IMasterScheduleSettingsService
{
    Task<ScheduleSettingsResponseDTO?> GetBookingSettingsAsync(
        int masterId,
        CancellationToken cancellationToken = default);

    Task<ScheduleSettingsResult> UpdateBookingSettingsAsync(
        int masterId,
        UpdateScheduleSettingsRequestDTO request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkingHoursDayResponseDTO>> GetWorkingHoursWeekAsync(
        int masterId,
        CancellationToken cancellationToken = default);

    Task<ScheduleSettingsResult> ReplaceWorkingHoursAsync(
        int masterId,
        IReadOnlyList<WorkingHoursDayRequestDTO> days,
        CancellationToken cancellationToken = default);

    Task<ScheduleSettingsResult> UpdateWorkingHoursDayAsync(
        int masterId,
        Weekday day,
        UpdateWorkingHoursDayRequestDTO request,
        CancellationToken cancellationToken = default);

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

    Task<ScheduleSettingsResult> UpdateFreeSlotIntervalMinutesAsync(
        int masterId,
        int intervalMinutes,
        CancellationToken cancellationToken = default);

    Task<ScheduleSettingsResult> CreateTimeOffAsync(
        int masterId,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TimeOffResponseDTO>> GetTimeOffsAsync(
        int masterId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);

    Task<(ScheduleSettingsResult Result, TimeOffResponseDTO? Created)> CreateTimeOffFromLocalAsync(
        int masterId,
        CreateTimeOffRequestDTO request,
        CancellationToken cancellationToken = default);

    Task<ScheduleSettingsResult> DeleteTimeOffForMasterAsync(
        int masterId,
        int timeOffId,
        CancellationToken cancellationToken = default);
}
