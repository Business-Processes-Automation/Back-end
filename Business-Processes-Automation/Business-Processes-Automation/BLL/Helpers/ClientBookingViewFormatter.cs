using System.Text;
using Business_Processes_Automation.BLL.DTOs.Schedule;
using Business_Processes_Automation.BLL.Enums;
using Business_Processes_Automation.BLL.Localization;
using Business_Processes_Automation.BLL.Results;
using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.Helpers;

public static class ClientBookingViewFormatter
{
    private static readonly AppointmentStatus[] BlockingStatuses =
    [
        AppointmentStatus.Planned,
        AppointmentStatus.Rescheduled
    ];

    public static ClientBookingViewResult FormatPeriodIntro(
        ScheduleViewPeriod period,
        DateOnly rangeStart,
        DateOnly rangeEnd,
        TimeZoneInfo timeZone,
        IReadOnlyList<WorkingHoursPerDay> workingHours,
        IReadOnlyList<TimeOff> timeOffs,
        IReadOnlyList<Appointment> appointments)
    {
        var builder = new StringBuilder();
        builder.AppendLine(GetPeriodTitle(period, rangeStart, rangeEnd));

        if (period == ScheduleViewPeriod.Tomorrow)
        {
            builder.AppendLine();
            AppendDayOverview(
                builder,
                rangeStart,
                timeZone,
                workingHours.ToDictionary(x => x.DayOfWeek),
                timeOffs,
                appointments);
        }

        return Wrap(builder);
    }

    public static ClientBookingViewResult FormatDaySlots(
        DateOnly date,
        TimeZoneInfo timeZone,
        Service service,
        IReadOnlyList<WorkingHoursPerDay> workingHours,
        IReadOnlyList<TimeOff> timeOffs,
        IReadOnlyList<Appointment> appointments,
        IReadOnlyList<FreeSlot> daySlots)
    {
        var builder = new StringBuilder();
        builder.AppendLine(ClientBookingMessages.DayServiceHeader(date, service.ServiceName));
        builder.AppendLine();

        AppendDayOverview(
            builder,
            date,
            timeZone,
            workingHours.ToDictionary(x => x.DayOfWeek),
            timeOffs,
            appointments);

        builder.AppendLine();
        if (daySlots.Count == 0)
        {
            builder.Append(ClientBookingMessages.NoSlotsOnDay);
        }
        else
        {
            builder.Append(ClientBookingMessages.ChooseSlot);
        }

        return Wrap(builder);
    }

    public static ClientBookingViewResult FormatOverview(
        ScheduleViewPeriod period,
        DateOnly rangeStart,
        DateOnly rangeEnd,
        TimeZoneInfo timeZone,
        IReadOnlyList<WorkingHoursPerDay> workingHours,
        IReadOnlyList<TimeOff> timeOffs,
        IReadOnlyList<Appointment> appointments,
        IReadOnlyList<Service> services)
    {
        var workingHoursByDay = workingHours.ToDictionary(x => x.DayOfWeek);
        var builder = new StringBuilder();
        builder.AppendLine(GetPeriodTitle(period, rangeStart, rangeEnd));
        builder.AppendLine();

        for (var date = rangeStart; date <= rangeEnd; date = date.AddDays(1))
        {
            AppendDayOverview(
                builder,
                date,
                timeZone,
                workingHoursByDay,
                timeOffs,
                appointments);
        }

        builder.AppendLine();
        builder.AppendLine(ClientBookingMessages.ServicesForBookingHeader);
        for (var i = 0; i < services.Count; i++)
        {
            builder.AppendLine(ScheduleDisplayHelper.FormatServiceLine(services[i], i + 1));
        }

        builder.AppendLine();
        builder.Append(ClientBookingMessages.EnterServiceNumber);

        return Wrap(builder);
    }

    public static ClientBookingViewResult FormatWithFreeSlots(
        ScheduleViewPeriod period,
        DateOnly rangeStart,
        DateOnly rangeEnd,
        TimeZoneInfo timeZone,
        Service service,
        IReadOnlyList<WorkingHoursPerDay> workingHours,
        IReadOnlyList<TimeOff> timeOffs,
        IReadOnlyList<Appointment> appointments,
        IReadOnlyList<FreeSlot> freeSlots)
    {
        var workingHoursByDay = workingHours.ToDictionary(x => x.DayOfWeek);
        var builder = new StringBuilder();
        builder.AppendLine(GetPeriodTitle(period, rangeStart, rangeEnd));
        builder.AppendLine(ClientBookingMessages.ServiceLine(service.ServiceName, service.DurationInMinutes));
        builder.AppendLine();

        for (var date = rangeStart; date <= rangeEnd; date = date.AddDays(1))
        {
            AppendDayOverview(
                builder,
                date,
                timeZone,
                workingHoursByDay,
                timeOffs,
                appointments);

            var daySlots = freeSlots.Where(x => x.LocalDate == date).ToList();
            if (daySlots.Count == 0)
            {
                builder.AppendLine(ClientBookingMessages.NoFreeSlotsLabel);
            }
            else
            {
                builder.AppendLine(ClientBookingMessages.FreeSlotsHeader);
                foreach (var slot in daySlots)
                {
                    builder.AppendLine($"{slot.ListIndex}) {slot.StartTime:HH:mm}");
                }
            }

            builder.AppendLine();
        }

        if (freeSlots.Count == 0)
        {
            builder.AppendLine(ClientBookingMessages.NoSlotsInPeriod);
        }
        else
        {
            builder.Append(ClientBookingMessages.EnterSlotNumber);
        }

        return Wrap(builder);
    }

    public static string FormatConfirmation(
        Service service,
        FreeSlot slot,
        TimeZoneInfo timeZone,
        decimal price,
        decimal prepayment)
    {
        var endLocal = MasterTimeZoneHelper.ToLocal(slot.EndUtc, timeZone);

        return ClientBookingMessages.ConfirmationHeader(
            service.ServiceName,
            slot.LocalDate,
            slot.StartTime,
            endLocal,
            service.DurationInMinutes,
            price,
            prepayment);
    }

    private static void AppendDayOverview(
        StringBuilder builder,
        DateOnly date,
        TimeZoneInfo timeZone,
        IReadOnlyDictionary<Weekday, WorkingHoursPerDay> workingHoursByDay,
        IReadOnlyList<TimeOff> timeOffs,
        IReadOnlyList<Appointment> appointments)
    {
        var weekday = (Weekday)(int)date.DayOfWeek;
        builder.AppendLine($"{date:dd.MM.yyyy} ({ScheduleDisplayHelper.GetDayLabel(weekday)})");

        var (dayStartUtc, dayEndUtc) = MasterTimeZoneHelper.GetDayBoundsUtc(date, timeZone);

        if (ScheduleDisplayHelper.IsFullDayOff(timeOffs, dayStartUtc, dayEndUtc))
        {
            builder.AppendLine(ClientBookingMessages.DayOff);
            return;
        }

        if (workingHoursByDay.TryGetValue(weekday, out var hours))
        {
            builder.AppendLine(ClientBookingMessages.WorkingHoursLine(hours.WorkStartTime, hours.WorkEndTime));
        }
        else
        {
            builder.AppendLine(ClientBookingMessages.MasterNotWorking);
            return;
        }

        var dayTimeOffs = timeOffs
            .Where(x => x.StartDateTime < dayEndUtc && x.EndDateTime > dayStartUtc
                        && !ScheduleDisplayHelper.IsFullDayOffBlock(x, dayStartUtc, dayEndUtc))
            .ToList();

        foreach (var block in dayTimeOffs)
        {
            var start = MasterTimeZoneHelper.ToLocal(block.StartDateTime, timeZone);
            var end = MasterTimeZoneHelper.ToLocal(block.EndDateTime, timeZone);
            builder.AppendLine(ClientBookingMessages.UnavailableBlock(start, end));
        }

        var dayAppointments = appointments
            .Where(x => BlockingStatuses.Contains(x.Status))
            .Where(x =>
            {
                var startLocal = MasterTimeZoneHelper.ToLocal(x.StartDateTime, timeZone);
                return DateOnly.FromDateTime(startLocal) == date;
            })
            .OrderBy(x => x.StartDateTime)
            .ToList();

        foreach (var appointment in dayAppointments)
        {
            var start = MasterTimeZoneHelper.ToLocal(appointment.StartDateTime, timeZone);
            var end = MasterTimeZoneHelper.ToLocal(appointment.EndDateTime, timeZone);
            builder.AppendLine(ClientBookingMessages.OccupiedBlock(start, end));
        }
    }

    private static ClientBookingViewResult Wrap(StringBuilder builder)
    {
        var fullText = builder.ToString().TrimEnd();
        return new ClientBookingViewResult
        {
            MessageParts = TelegramMessageSplitter.Split(fullText)
        };
    }

    private static string GetPeriodTitle(ScheduleViewPeriod period, DateOnly start, DateOnly end) =>
        period switch
        {
            ScheduleViewPeriod.Tomorrow => ClientBookingMessages.PeriodTitleTomorrow(start),
            ScheduleViewPeriod.ThreeDays => ClientBookingMessages.PeriodTitleThreeDays(start, end),
            ScheduleViewPeriod.Week => ClientBookingMessages.PeriodTitleWeek(start, end),
            ScheduleViewPeriod.Month => ClientBookingMessages.PeriodTitleMonth(start, end),
            _ => ClientBookingMessages.DefaultPeriodTitle
        };
}
