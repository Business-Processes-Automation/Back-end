using Business_Processes_Automation.BLL.Helpers;
using Business_Processes_Automation.BLL.Interfaces;
using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.BLL.Localization;
using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.Services;

public class MasterScheduleApiService : IMasterScheduleApiService
{
    private static readonly AppointmentStatus[] DefaultCalendarStatuses =
    [
        AppointmentStatus.Planned,
        AppointmentStatus.Rescheduled,
        AppointmentStatus.Completed,
        AppointmentStatus.NoShow
    ];

    private readonly IMasterRepository _masterRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IMasterScheduleSettingsService _scheduleSettingsService;
    private readonly IMasterAppointmentService _appointmentService;

    public MasterScheduleApiService(
        IMasterRepository masterRepository,
        IAppointmentRepository appointmentRepository,
        IMasterScheduleSettingsService scheduleSettingsService,
        IMasterAppointmentService appointmentService)
    {
        _masterRepository = masterRepository;
        _appointmentRepository = appointmentRepository;
        _scheduleSettingsService = scheduleSettingsService;
        _appointmentService = appointmentService;
    }

    public async Task<CalendarResponseDTO> GetCalendarAsync(
        int masterId,
        DateOnly from,
        DateOnly to,
        bool includeCancelled = false,
        AppointmentStatus? status = null,
        int? serviceId = null,
        CancellationToken cancellationToken = default)
    {
        if (to < from)
        {
            throw new InvalidOperationException(ScheduleSettingsMessages.TimeOffDateRangeInvalid);
        }

        var master = await _masterRepository.GetByIdAsync(masterId, cancellationToken)
            ?? throw new InvalidOperationException(AuthMessages.MasterAccountNotFound);

        var timeZone = MasterTimeZoneHelper.ResolveTimeZone(master.TimeZone);
        var (fromUtc, _) = MasterTimeZoneHelper.GetDayBoundsUtc(from, timeZone);
        var (_, toUtc) = MasterTimeZoneHelper.GetDayBoundsUtc(to, timeZone);

        var workingHours = await _scheduleSettingsService.GetWorkingHoursAsync(masterId, cancellationToken);
        var workingHoursByDay = workingHours.ToDictionary(x => x.DayOfWeek);

        var timeOffs = await _scheduleSettingsService.GetTimeOffsInRangeAsync(
            masterId,
            fromUtc,
            toUtc,
            cancellationToken);

        var statuses = ResolveCalendarStatuses(includeCancelled, status);

        var appointments = await _appointmentRepository.GetByMasterIdInRangeAsync(
            masterId,
            fromUtc,
            toUtc,
            statuses,
            serviceId,
            cancellationToken);

        var days = new List<CalendarDayResponseDTO>();

        for (var date = from; date <= to; date = date.AddDays(1))
        {
            days.Add(BuildCalendarDay(date, timeZone, workingHoursByDay, timeOffs, appointments));
        }

        return new CalendarResponseDTO
        {
            TimeZone = master.TimeZone,
            From = from,
            To = to,
            Days = days
        };
    }

    public Task<FreeSlotsResponseDTO> GetFreeSlotsAsync(
        int masterId,
        DateOnly from,
        DateOnly to,
        int serviceId,
        CancellationToken cancellationToken = default) =>
        _appointmentService.GetFreeSlotsResponseAsync(
            masterId,
            from,
            to,
            serviceId,
            cancellationToken);

    public async Task<AppointmentDetailsResponseDTO?> GetAppointmentDetailsAsync(
        int masterId,
        int appointmentId,
        CancellationToken cancellationToken = default)
    {
        var appointment = await _appointmentRepository.GetByIdForMasterAsync(
            appointmentId,
            masterId,
            cancellationToken);

        if (appointment is null || appointment.IsDeleted)
        {
            return null;
        }

        var master = await _masterRepository.GetByIdAsync(masterId, cancellationToken)
            ?? throw new InvalidOperationException(AuthMessages.MasterAccountNotFound);

        var timeZone = MasterTimeZoneHelper.ResolveTimeZone(master.TimeZone);
        return AppointmentMapper.MapToDetails(appointment, timeZone);
    }

    private static IReadOnlyCollection<AppointmentStatus> ResolveCalendarStatuses(
        bool includeCancelled,
        AppointmentStatus? status)
    {
        if (status.HasValue)
        {
            return [status.Value];
        }

        if (includeCancelled)
        {
            return
            [
                AppointmentStatus.Planned,
                AppointmentStatus.Rescheduled,
                AppointmentStatus.Completed,
                AppointmentStatus.NoShow,
                AppointmentStatus.Cancelled
            ];
        }

        return DefaultCalendarStatuses;
    }

    private static CalendarDayResponseDTO BuildCalendarDay(
        DateOnly date,
        TimeZoneInfo timeZone,
        IReadOnlyDictionary<Weekday, WorkingHoursPerDay> workingHoursByDay,
        IReadOnlyList<TimeOff> timeOffs,
        IReadOnlyList<Appointment> appointments)
    {
        var weekday = (Weekday)(int)date.DayOfWeek;
        var (dayStartUtc, dayEndUtc) = MasterTimeZoneHelper.GetDayBoundsUtc(date, timeZone);

        CalendarWorkingHoursDTO? workingHours = null;
        if (workingHoursByDay.TryGetValue(weekday, out var hours)
            && !ScheduleDisplayHelper.IsFullDayOff(timeOffs, dayStartUtc, dayEndUtc))
        {
            workingHours = new CalendarWorkingHoursDTO
            {
                Start = WorkingHoursHelper.FormatTime(hours.WorkStartTime),
                End = WorkingHoursHelper.FormatTime(hours.WorkEndTime)
            };
        }

        var dayTimeOffs = timeOffs
            .Where(x => x.StartDateTime < dayEndUtc && x.EndDateTime > dayStartUtc)
            .Select(x => TimeOffHelper.MapToDto(x, timeZone))
            .ToList();

        var dayAppointments = appointments
            .Where(x => DateOnly.FromDateTime(MasterTimeZoneHelper.ToLocal(x.StartDateTime, timeZone)) == date)
            .OrderBy(x => x.StartDateTime)
            .Select(x => MapAppointmentSummary(x, timeZone))
            .ToList();

        return new CalendarDayResponseDTO
        {
            Date = date,
            WorkingHours = workingHours,
            TimeOffs = dayTimeOffs,
            Appointments = dayAppointments
        };
    }

    private static CalendarAppointmentSummaryDTO MapAppointmentSummary(
        Appointment appointment,
        TimeZoneInfo timeZone) =>
        new()
        {
            Id = appointment.Id,
            ServiceId = appointment.ServiceId,
            StartLocal = MasterTimeZoneHelper.ToLocal(appointment.StartDateTime, timeZone),
            EndLocal = MasterTimeZoneHelper.ToLocal(appointment.EndDateTime, timeZone),
            ServiceName = appointment.Service.ServiceName,
            ClientName = appointment.Client.ClientName,
            Status = appointment.Status
        };
}
