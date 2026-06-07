using Business_Processes_Automation.BLL.Helpers;
using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.BLL.Localization;
using Business_Processes_Automation.BLL.Results;
using Business_Processes_Automation.DAL;
using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace Business_Processes_Automation.BLL.Services;

public class MasterScheduleSettingsService : IMasterScheduleSettingsService
{
    private readonly AppDbContext _dbContext;
    private readonly IMasterRepository _masterRepository;
    private readonly IWorkingHoursPerDayRepository _workingHoursRepository;
    private readonly IMasterAppointmentSettingRepository _settingsRepository;
    private readonly ITimeOffRepository _timeOffRepository;

    public MasterScheduleSettingsService(
        AppDbContext dbContext,
        IMasterRepository masterRepository,
        IWorkingHoursPerDayRepository workingHoursRepository,
        IMasterAppointmentSettingRepository settingsRepository,
        ITimeOffRepository timeOffRepository)
    {
        _dbContext = dbContext;
        _masterRepository = masterRepository;
        _workingHoursRepository = workingHoursRepository;
        _settingsRepository = settingsRepository;
        _timeOffRepository = timeOffRepository;
    }

    public async Task<ScheduleSettingsResponseDTO?> GetBookingSettingsAsync(
        int masterId,
        CancellationToken cancellationToken = default)
    {
        var setting = await _settingsRepository.GetByMasterIdAsync(masterId, cancellationToken);
        return setting is null ? null : MapToBookingSettingsDto(setting);
    }

    public async Task<ScheduleSettingsResult> UpdateBookingSettingsAsync(
        int masterId,
        UpdateScheduleSettingsRequestDTO request,
        CancellationToken cancellationToken = default)
    {
        if (request.BufferBetweenClientsMinutes is < 0 or > 480)
        {
            return ScheduleSettingsResult.Fail(ScheduleSettingsMessages.BufferOutOfRange);
        }

        if (request.FreeSlotIntervalMinutes is < 5 or > 120
            || request.FreeSlotIntervalMinutes % 5 != 0)
        {
            return ScheduleSettingsResult.Fail(ScheduleSettingsMessages.SlotIntervalOutOfRange);
        }

        if (request.MinBookingNoticeMinutes is < 0 or > 10080)
        {
            return ScheduleSettingsResult.Fail(ScheduleSettingsMessages.MinBookingNoticeOutOfRange);
        }

        if (request.MaxBookingDaysAhead is < 1 or > 365)
        {
            return ScheduleSettingsResult.Fail(ScheduleSettingsMessages.MaxBookingDaysAheadOutOfRange);
        }

        if (request.MaxRescheduleCount is < 0 or > 10)
        {
            return ScheduleSettingsResult.Fail(ScheduleSettingsMessages.MaxRescheduleCountOutOfRange);
        }

        if (request.CancellationPolicyHours is < 0 or > 168)
        {
            return ScheduleSettingsResult.Fail(ScheduleSettingsMessages.CancellationPolicyHoursOutOfRange);
        }

        var setting = await _settingsRepository.GetByMasterIdAsync(masterId, cancellationToken);

        if (setting is null)
        {
            return ScheduleSettingsResult.Fail(ScheduleSettingsMessages.AppointmentSettingsNotFound);
        }

        setting.BufferBetweenClientsMinutes = request.BufferBetweenClientsMinutes;
        setting.FreeSlotIntervalMinutes = request.FreeSlotIntervalMinutes;
        setting.MinBookingNoticeMinutes = request.MinBookingNoticeMinutes;
        setting.MaxBookingDaysAhead = request.MaxBookingDaysAhead;
        setting.MaxRescheduleCount = request.MaxRescheduleCount;
        setting.CancellationPolicyHours = request.CancellationPolicyHours;

        var updated = await _settingsRepository.UpdateAsync(setting, cancellationToken);
        return ScheduleSettingsResult.Ok(updated);
    }

    public async Task<IReadOnlyList<WorkingHoursDayResponseDTO>> GetWorkingHoursWeekAsync(
        int masterId,
        CancellationToken cancellationToken = default)
    {
        var workingHours = await _workingHoursRepository.GetByMasterIdAsync(masterId, cancellationToken);
        return WorkingHoursHelper.MapWeek(workingHours);
    }

    public async Task<ScheduleSettingsResult> ReplaceWorkingHoursAsync(
        int masterId,
        IReadOnlyList<WorkingHoursDayRequestDTO> days,
        CancellationToken cancellationToken = default)
    {
        var validationError = WorkingHoursHelper.ValidateWeekDays(days);
        if (validationError is not null)
        {
            return ScheduleSettingsResult.Fail(validationError);
        }

        var strategy = _dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async ct =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

            var existingRows = await _dbContext.WorkingHoursPerDays
                .Where(x => x.MasterId == masterId)
                .ToListAsync(ct);

            var byDay = existingRows.ToDictionary(x => x.DayOfWeek);

            foreach (var dayRequest in days)
            {
                ApplyWorkingHoursDay(
                    masterId,
                    dayRequest.DayOfWeek,
                    dayRequest.IsWorking,
                    dayRequest.WorkStartTime,
                    dayRequest.WorkEndTime,
                    byDay,
                    _dbContext);
            }

            await _dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }, cancellationToken);

        return ScheduleSettingsResult.Ok();
    }

    public async Task<ScheduleSettingsResult> UpdateWorkingHoursDayAsync(
        int masterId,
        Weekday day,
        UpdateWorkingHoursDayRequestDTO request,
        CancellationToken cancellationToken = default)
    {
        var validationError = WorkingHoursHelper.ValidateDayRequest(request);
        if (validationError is not null)
        {
            return ScheduleSettingsResult.Fail(validationError);
        }

        if (!request.IsWorking)
        {
            return await DeleteWorkingHoursForDayAsync(masterId, day, cancellationToken);
        }

        if (!WorkingHoursHelper.TryParseTime(request.WorkStartTime!, out var workStartTime)
            || !WorkingHoursHelper.TryParseTime(request.WorkEndTime!, out var workEndTime))
        {
            return ScheduleSettingsResult.Fail(ScheduleSettingsMessages.InvalidWorkTimeFormat);
        }

        return await UpsertWorkingHoursAsync(
            masterId,
            day,
            workStartTime,
            workEndTime,
            cancellationToken);
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
        var validationError = WorkingHoursHelper.ValidateWorkingTimes(
            WorkingHoursHelper.FormatTime(workStartTime),
            WorkingHoursHelper.FormatTime(workEndTime));

        if (validationError is not null)
        {
            return ScheduleSettingsResult.Fail(validationError);
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

    public async Task<IReadOnlyList<TimeOffResponseDTO>> GetTimeOffsAsync(
        int masterId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        if (to < from)
        {
            throw new InvalidOperationException(ScheduleSettingsMessages.TimeOffDateRangeInvalid);
        }

        var timeZone = await GetMasterTimeZoneAsync(masterId, cancellationToken);
        var (fromUtc, _) = MasterTimeZoneHelper.GetDayBoundsUtc(from, timeZone);
        var (_, toUtc) = MasterTimeZoneHelper.GetDayBoundsUtc(to, timeZone);

        var timeOffs = await _timeOffRepository.GetByMasterIdInRangeAsync(
            masterId,
            fromUtc,
            toUtc,
            cancellationToken);

        return timeOffs
            .Select(x => TimeOffHelper.MapToDto(x, timeZone))
            .ToList();
    }

    public async Task<(ScheduleSettingsResult Result, TimeOffResponseDTO? Created)> CreateTimeOffFromLocalAsync(
        int masterId,
        CreateTimeOffRequestDTO request,
        CancellationToken cancellationToken = default)
    {
        var timeZone = await GetMasterTimeZoneAsync(masterId, cancellationToken);

        var (startLocal, endLocal) = TimeOffHelper.NormalizeLocalRange(
            request.StartLocal,
            request.EndLocal,
            request.IsFullDay);

        if (endLocal <= startLocal)
        {
            return (ScheduleSettingsResult.Fail(ScheduleSettingsMessages.EndTimeMustBeAfterStart), null);
        }

        var startUtc = MasterTimeZoneHelper.ToUtc(startLocal, timeZone);
        var endUtc = MasterTimeZoneHelper.ToUtc(endLocal, timeZone);

        var result = await CreateTimeOffAsync(masterId, startUtc, endUtc, cancellationToken);

        if (!result.Success || result.TimeOff is null)
        {
            return (result, null);
        }

        return (result, TimeOffHelper.MapToDto(result.TimeOff, timeZone));
    }

    public async Task<ScheduleSettingsResult> DeleteTimeOffForMasterAsync(
        int masterId,
        int timeOffId,
        CancellationToken cancellationToken = default)
    {
        var timeOff = await _timeOffRepository.GetByIdAsync(timeOffId, cancellationToken);

        if (timeOff is null || timeOff.MasterId != masterId)
        {
            return ScheduleSettingsResult.Fail(ScheduleSettingsMessages.TimeOffNotFound);
        }

        await _timeOffRepository.SoftDeleteAsync(timeOffId, cancellationToken);
        return ScheduleSettingsResult.Ok();
    }

    private async Task<TimeZoneInfo> GetMasterTimeZoneAsync(
        int masterId,
        CancellationToken cancellationToken)
    {
        var master = await _masterRepository.GetByIdAsync(masterId, cancellationToken)
            ?? throw new InvalidOperationException(AuthMessages.MasterAccountNotFound);

        return MasterTimeZoneHelper.ResolveTimeZone(master.TimeZone);
    }

    private static ScheduleSettingsResponseDTO MapToBookingSettingsDto(MasterAppointmentSetting setting) =>
        new()
        {
            BufferBetweenClientsMinutes = setting.BufferBetweenClientsMinutes,
            FreeSlotIntervalMinutes = setting.FreeSlotIntervalMinutes,
            MinBookingNoticeMinutes = setting.MinBookingNoticeMinutes,
            MaxBookingDaysAhead = setting.MaxBookingDaysAhead,
            MaxRescheduleCount = setting.MaxRescheduleCount,
            CancellationPolicyHours = setting.CancellationPolicyHours
        };

    private static void ApplyWorkingHoursDay(
        int masterId,
        Weekday day,
        bool isWorking,
        string? workStartTime,
        string? workEndTime,
        IDictionary<Weekday, WorkingHoursPerDay> byDay,
        AppDbContext dbContext)
    {
        if (!isWorking)
        {
            if (byDay.TryGetValue(day, out var dayOffRow))
            {
                dayOffRow.IsDeleted = true;
                dayOffRow.UpdatedAt = DateTime.UtcNow;
            }

            return;
        }

        if (!WorkingHoursHelper.TryParseTime(workStartTime!, out var start)
            || !WorkingHoursHelper.TryParseTime(workEndTime!, out var end))
        {
            throw new InvalidOperationException(ScheduleSettingsMessages.InvalidWorkTimeFormat);
        }

        if (byDay.TryGetValue(day, out var existingRow))
        {
            existingRow.WorkStartTime = start;
            existingRow.WorkEndTime = end;
            existingRow.IsDeleted = false;
            existingRow.UpdatedAt = DateTime.UtcNow;
            return;
        }

        var created = new WorkingHoursPerDay
        {
            MasterId = masterId,
            DayOfWeek = day,
            WorkStartTime = start,
            WorkEndTime = end,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.WorkingHoursPerDays.Add(created);
        byDay[day] = created;
    }
}
