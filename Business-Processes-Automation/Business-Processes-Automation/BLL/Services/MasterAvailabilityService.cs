using Business_Processes_Automation.BLL.DTOs.Schedule;
using Business_Processes_Automation.BLL.Helpers;
using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.Services;

public class MasterAvailabilityService : IMasterAvailabilityService
{
    private const int SlotStepMinutes = 15;

    private static readonly AppointmentStatus[] BlockingStatuses =
    [
        AppointmentStatus.Planned,
        AppointmentStatus.Rescheduled
    ];

    private readonly IMasterRepository _masterRepository;
    private readonly IWorkingHoursPerDayRepository _workingHoursRepository;
    private readonly IMasterAppointmentSettingRepository _settingsRepository;
    private readonly ITimeOffRepository _timeOffRepository;
    private readonly IAppointmentRepository _appointmentRepository;

    public MasterAvailabilityService(
        IMasterRepository masterRepository,
        IWorkingHoursPerDayRepository workingHoursRepository,
        IMasterAppointmentSettingRepository settingsRepository,
        ITimeOffRepository timeOffRepository,
        IAppointmentRepository appointmentRepository)
    {
        _masterRepository = masterRepository;
        _workingHoursRepository = workingHoursRepository;
        _settingsRepository = settingsRepository;
        _timeOffRepository = timeOffRepository;
        _appointmentRepository = appointmentRepository;
    }

    public async Task<TimeZoneInfo> GetMasterTimeZoneAsync(
        int masterId,
        CancellationToken cancellationToken = default)
    {
        var master = await _masterRepository.GetByIdAsync(masterId, cancellationToken)
            ?? throw new InvalidOperationException($"Master {masterId} was not found.");

        return MasterTimeZoneHelper.ResolveTimeZone(master.TimeZone);
    }

    public Task<IReadOnlyList<WorkingHoursPerDay>> GetWorkingHoursForMasterAsync(
        int masterId,
        CancellationToken cancellationToken = default) =>
        _workingHoursRepository.GetByMasterIdAsync(masterId, cancellationToken);

    public Task<MasterAppointmentSetting?> GetSettingsForMasterAsync(
        int masterId,
        CancellationToken cancellationToken = default) =>
        _settingsRepository.GetByMasterIdAsync(masterId, cancellationToken);

    public Task<IReadOnlyList<TimeOff>> GetTimeOffsAsync(
        int masterId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default) =>
        _timeOffRepository.GetByMasterIdInRangeAsync(masterId, fromUtc, toUtc, cancellationToken);

    public Task<IReadOnlyList<Appointment>> GetAppointmentsAsync(
        int masterId,
        DateTime fromUtc,
        DateTime toUtc,
        IReadOnlyCollection<AppointmentStatus>? statuses = null,
        CancellationToken cancellationToken = default) =>
        _appointmentRepository.GetByMasterIdInRangeAsync(
            masterId,
            fromUtc,
            toUtc,
            statuses,
            cancellationToken);

    public async Task<IReadOnlyList<FreeSlot>> GetFreeSlotsForPeriodAsync(
        int masterId,
        DateOnly rangeStart,
        DateOnly rangeEnd,
        int durationMinutes,
        CancellationToken cancellationToken = default)
    {
        var timeZone = await GetMasterTimeZoneAsync(masterId, cancellationToken);
        var settings = await GetSettingsForMasterAsync(masterId, cancellationToken);
        var workingHours = await GetWorkingHoursForMasterAsync(masterId, cancellationToken);
        var workingHoursByDay = workingHours.ToDictionary(x => x.DayOfWeek);

        var (periodFromUtc, _) = MasterTimeZoneHelper.GetDayBoundsUtc(rangeStart, timeZone);
        var (_, periodToUtc) = MasterTimeZoneHelper.GetDayBoundsUtc(rangeEnd, timeZone);

        var timeOffs = await GetTimeOffsAsync(masterId, periodFromUtc, periodToUtc, cancellationToken);
        var appointments = await GetAppointmentsAsync(
            masterId,
            periodFromUtc,
            periodToUtc,
            BlockingStatuses,
            cancellationToken);

        var bufferMinutes = settings?.BufferBetweenClientsMinutes ?? 0;
        var minNoticeMinutes = settings?.MinBookingNoticeMinutes ?? 0;
        var maxDaysAhead = settings?.MaxBookingDaysAhead ?? 30;
        var earliestUtc = DateTime.UtcNow.AddMinutes(minNoticeMinutes);
        var today = MasterTimeZoneHelper.ToLocalDate(DateTime.UtcNow, timeZone);
        var lastBookableDate = today.AddDays(maxDaysAhead);

        var slots = new List<FreeSlot>();
        var listIndex = 1;

        for (var date = rangeStart; date <= rangeEnd; date = date.AddDays(1))
        {
            if (date > lastBookableDate)
            {
                continue;
            }

            var daySlots = BuildSlotsForDay(
                date,
                timeZone,
                workingHoursByDay,
                timeOffs,
                appointments,
                durationMinutes,
                bufferMinutes,
                earliestUtc);

            foreach (var slot in daySlots)
            {
                slots.Add(new FreeSlot
                {
                    ListIndex = listIndex++,
                    LocalDate = date,
                    StartTime = TimeOnly.FromDateTime(MasterTimeZoneHelper.ToLocal(slot.StartUtc, timeZone)),
                    StartUtc = slot.StartUtc,
                    EndUtc = slot.EndUtc
                });
            }
        }

        return slots;
    }

    public async Task<IReadOnlyList<TimeOnly>> GetFreeSlotsAsync(
        int masterId,
        DateOnly localDate,
        int durationMinutes,
        CancellationToken cancellationToken = default)
    {
        var slots = await GetFreeSlotsForPeriodAsync(
            masterId,
            localDate,
            localDate,
            durationMinutes,
            cancellationToken);

        return slots.Select(x => x.StartTime).ToList();
    }

    private static List<(DateTime StartUtc, DateTime EndUtc)> BuildSlotsForDay(
        DateOnly date,
        TimeZoneInfo timeZone,
        IReadOnlyDictionary<Weekday, WorkingHoursPerDay> workingHoursByDay,
        IReadOnlyList<TimeOff> timeOffs,
        IReadOnlyList<Appointment> appointments,
        int durationMinutes,
        int bufferMinutes,
        DateTime earliestUtc)
    {
        var weekday = (Weekday)(int)date.DayOfWeek;
        var (dayStartUtc, dayEndUtc) = MasterTimeZoneHelper.GetDayBoundsUtc(date, timeZone);

        if (IsFullDayOff(timeOffs, dayStartUtc, dayEndUtc))
        {
            return [];
        }

        if (!workingHoursByDay.TryGetValue(weekday, out var hours))
        {
            return [];
        }

        var workStartLocal = date.ToDateTime(hours.WorkStartTime);
        var workEndLocal = date.ToDateTime(hours.WorkEndTime);
        var workStartUtc = MasterTimeZoneHelper.ToUtc(workStartLocal, timeZone);
        var workEndUtc = MasterTimeZoneHelper.ToUtc(workEndLocal, timeZone);

        var freeRanges = new List<TimeRange> { new(workStartUtc, workEndUtc) };

        var busyRanges = new List<TimeRange>();

        foreach (var timeOff in timeOffs.Where(x => x.StartDateTime < dayEndUtc && x.EndDateTime > dayStartUtc))
        {
            busyRanges.Add(new TimeRange(timeOff.StartDateTime, timeOff.EndDateTime));
        }

        foreach (var appointment in appointments)
        {
            var busyStart = appointment.StartDateTime;
            var busyEnd = appointment.EndDateTime.AddMinutes(bufferMinutes);
            if (busyEnd > dayStartUtc && busyStart < dayEndUtc)
            {
                busyRanges.Add(new TimeRange(busyStart, busyEnd));
            }
        }

        freeRanges = AvailabilityIntervalHelper.SubtractAll(freeRanges, busyRanges).ToList();

        var duration = TimeSpan.FromMinutes(durationMinutes);
        var step = TimeSpan.FromMinutes(SlotStepMinutes);
        var result = new List<(DateTime StartUtc, DateTime EndUtc)>();

        foreach (var free in freeRanges)
        {
            var candidate = free.Start;

            while (candidate.Add(duration) <= free.End)
            {
                if (candidate >= earliestUtc)
                {
                    result.Add((candidate, candidate.Add(duration)));
                }

                candidate = candidate.Add(step);
            }
        }

        return result;
    }

    private static bool IsFullDayOff(IReadOnlyList<TimeOff> timeOffs, DateTime dayStartUtc, DateTime dayEndUtc) =>
        timeOffs.Any(x => x.StartDateTime <= dayStartUtc && x.EndDateTime >= dayEndUtc);
}
