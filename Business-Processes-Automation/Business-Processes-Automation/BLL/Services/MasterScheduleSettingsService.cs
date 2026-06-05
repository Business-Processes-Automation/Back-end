using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.BLL.Localization;
using Business_Processes_Automation.BLL.Results;
using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.Services;

public class MasterScheduleSettingsService : IMasterScheduleSettingsService
{
    private readonly IWorkingHoursPerDayRepository _workingHoursRepository;
    private readonly IMasterAppointmentSettingRepository _settingsRepository;
    private readonly ITimeOffRepository _timeOffRepository;

    public MasterScheduleSettingsService(
        IWorkingHoursPerDayRepository workingHoursRepository,
        IMasterAppointmentSettingRepository settingsRepository,
        ITimeOffRepository timeOffRepository)
    {
        _workingHoursRepository = workingHoursRepository;
        _settingsRepository = settingsRepository;
        _timeOffRepository = timeOffRepository;
    }

    public Task<IReadOnlyList<WorkingHoursPerDay>> GetWorkingHoursAsync(
        int masterId,
        CancellationToken cancellationToken = default) =>
        _workingHoursRepository.GetByMasterIdAsync(masterId, cancellationToken);

    public Task<MasterAppointmentSetting?> GetSettingAsync(
        int masterId,
        CancellationToken cancellationToken = default) =>
        _settingsRepository.GetByMasterIdAsync(masterId, cancellationToken);

    public Task<IReadOnlyList<TimeOff>> GetTimeOffsInRangeAsync(
        int masterId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default) =>
        _timeOffRepository.GetByMasterIdInRangeAsync(masterId, fromUtc, toUtc, cancellationToken);

    public async Task<ScheduleSettingsResult> UpsertWorkingHoursAsync(
        int masterId,
        Weekday day,
        TimeOnly workStartTime,
        TimeOnly workEndTime,
        CancellationToken cancellationToken = default)
    {
        if (workEndTime <= workStartTime)
        {
            return ScheduleSettingsResult.Fail(ScheduleSettingsMessages.EndTimeMustBeAfterStart);
        }

        var existing = await _workingHoursRepository.GetByMasterAndDayAsync(masterId, day, cancellationToken);

        if (existing is not null)
        {
            existing.WorkStartTime = workStartTime;
            existing.WorkEndTime = workEndTime;
            var updated = await _workingHoursRepository.UpdateAsync(existing, cancellationToken);
            return ScheduleSettingsResult.Ok(updated);
        }

        var created = await _workingHoursRepository.CreateAsync(
            new WorkingHoursPerDay
            {
                MasterId = masterId,
                DayOfWeek = day,
                WorkStartTime = workStartTime,
                WorkEndTime = workEndTime
            },
            cancellationToken);

        return ScheduleSettingsResult.Ok(created);
    }

    public async Task<ScheduleSettingsResult> DeleteWorkingHoursForDayAsync(
        int masterId,
        Weekday day,
        CancellationToken cancellationToken = default)
    {
        var existing = await _workingHoursRepository.GetByMasterAndDayAsync(masterId, day, cancellationToken);

        if (existing is null)
        {
            return ScheduleSettingsResult.Ok();
        }

        await _workingHoursRepository.SoftDeleteAsync(existing.Id, cancellationToken);
        return ScheduleSettingsResult.Ok();
    }

    public async Task<ScheduleSettingsResult> UpdateBufferMinutesAsync(
        int masterId,
        int bufferMinutes,
        CancellationToken cancellationToken = default)
    {
        if (bufferMinutes is < 0 or > 480)
        {
            return ScheduleSettingsResult.Fail(ScheduleSettingsMessages.BufferOutOfRange);
        }

        var setting = await _settingsRepository.GetByMasterIdAsync(masterId, cancellationToken);

        if (setting is null)
        {
            return ScheduleSettingsResult.Fail(ScheduleSettingsMessages.AppointmentSettingsNotFound);
        }

        setting.BufferBetweenClientsMinutes = bufferMinutes;
        var updated = await _settingsRepository.UpdateAsync(setting, cancellationToken);
        return ScheduleSettingsResult.Ok(updated);
    }

    public async Task<ScheduleSettingsResult> UpdateFreeSlotIntervalMinutesAsync(
        int masterId,
        int intervalMinutes,
        CancellationToken cancellationToken = default)
    {
        if (intervalMinutes is < 5 or > 120 || intervalMinutes % 5 != 0)
        {
            return ScheduleSettingsResult.Fail(ScheduleSettingsMessages.SlotIntervalOutOfRange);
        }

        var setting = await _settingsRepository.GetByMasterIdAsync(masterId, cancellationToken);

        if (setting is null)
        {
            return ScheduleSettingsResult.Fail(ScheduleSettingsMessages.AppointmentSettingsNotFound);
        }

        setting.FreeSlotIntervalMinutes = intervalMinutes;
        var updated = await _settingsRepository.UpdateAsync(setting, cancellationToken);
        return ScheduleSettingsResult.Ok(updated);
    }

    public async Task<ScheduleSettingsResult> CreateTimeOffAsync(
        int masterId,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken = default)
    {
        if (endUtc <= startUtc)
        {
            return ScheduleSettingsResult.Fail(ScheduleSettingsMessages.EndTimeMustBeAfterStart);
        }

        if (await _timeOffRepository.HasOverlapAsync(masterId, startUtc, endUtc, cancellationToken: cancellationToken))
        {
            return ScheduleSettingsResult.Fail(ScheduleSettingsMessages.TimeOffOverlap);
        }

        var created = await _timeOffRepository.CreateAsync(
            new TimeOff
            {
                MasterId = masterId,
                StartDateTime = startUtc,
                EndDateTime = endUtc
            },
            cancellationToken);

        return ScheduleSettingsResult.Ok(created);
    }

    public Task<bool> DeleteTimeOffAsync(int timeOffId, CancellationToken cancellationToken = default) =>
        _timeOffRepository.SoftDeleteAsync(timeOffId, cancellationToken);
}
