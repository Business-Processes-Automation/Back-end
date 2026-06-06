using Business_Processes_Automation.BLL.DTOs.Schedule;
using Business_Processes_Automation.BLL.Helpers;
using Business_Processes_Automation.BLL.Interfaces;
using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.BLL.Localization;
using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.Services;

public class MasterScheduleApiService : IMasterScheduleApiService
{
    private static readonly AppointmentStatus[] CalendarAppointmentStatuses =
    [
        AppointmentStatus.Planned,
        AppointmentStatus.Rescheduled,
        AppointmentStatus.Completed,
        AppointmentStatus.NoShow
    ];

    private readonly IMasterRepository _masterRepository;
    private readonly IMasterScheduleSettingsService _scheduleSettingsService;
    private readonly IMasterAvailabilityService _availabilityService;
    private readonly IAppointmentRepository _appointmentRepository;

    public MasterScheduleApiService(
        IMasterRepository masterRepository,
        IMasterScheduleSettingsService scheduleSettingsService,
        IMasterAvailabilityService availabilityService,
        IAppointmentRepository appointmentRepository)
    {
        _masterRepository = masterRepository;
        _scheduleSettingsService = scheduleSettingsService;
        _availabilityService = availabilityService;
        _appointmentRepository = appointmentRepository;
    }

    public async Task<CalendarResponseDTO> GetCalendarAsync(
        int masterId,
        DateOnly from,
        DateOnly to,
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

        var appointments = await _availabilityService.GetAppointmentsAsync(
            masterId,
            fromUtc,
            toUtc,
            CalendarAppointmentStatuses,
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

    public async Task<AppointmentDetailsResponseDTO?> GetAppointmentDetailsAsync(
        int masterId,
        int appointmentId,
        CancellationToken cancellationToken = default)
    {
        var appointment = await _appointmentRepository.GetByIdForMasterAsync(
            appointmentId,
            masterId,
            cancellationToken);

        if (appointment is null)
        {
            return null;
        }

        var master = await _masterRepository.GetByIdAsync(masterId, cancellationToken)
            ?? throw new InvalidOperationException(AuthMessages.MasterAccountNotFound);

        var timeZone = MasterTimeZoneHelper.ResolveTimeZone(master.TimeZone);
        var startLocal = MasterTimeZoneHelper.ToLocal(appointment.StartDateTime, timeZone);
        var endLocal = MasterTimeZoneHelper.ToLocal(appointment.EndDateTime, timeZone);

        return new AppointmentDetailsResponseDTO
        {
            Id = appointment.Id,
            StartLocal = startLocal,
            EndLocal = endLocal,
            ServiceName = appointment.Service.ServiceName,
            DurationInMinutes = appointment.Service.DurationInMinutes,
            ClientName = appointment.Client.ClientName,
            ClientPhone = appointment.Client.ClientPhone,
            Status = appointment.Status,
            PriceAtBooking = appointment.PriceAtBooking,
            PrepaymentAmount = appointment.PrepaymentAmount
        };
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
            StartLocal = MasterTimeZoneHelper.ToLocal(appointment.StartDateTime, timeZone),
            EndLocal = MasterTimeZoneHelper.ToLocal(appointment.EndDateTime, timeZone),
            ServiceName = appointment.Service.ServiceName,
            ClientName = appointment.Client.ClientName,
            Status = appointment.Status
        };
}
