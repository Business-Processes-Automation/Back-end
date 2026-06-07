using System.Text;
using Business_Processes_Automation.BLL.Enums;
using Business_Processes_Automation.BLL.Localization;
using Business_Processes_Automation.BLL.Results;
using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.Helpers;

public static class MasterScheduleTextFormatter
{
    private static readonly AppointmentStatus[] ViewStatuses =
    [
        AppointmentStatus.Planned,
        AppointmentStatus.Rescheduled,
        AppointmentStatus.Completed,
        AppointmentStatus.NoShow
    ];

    public static IReadOnlyList<AppointmentStatus> GetViewStatuses() => ViewStatuses;

    public static MasterScheduleViewResult FormatPeriod(
        ScheduleViewPeriod period,
        DateOnly rangeStart,
        DateOnly rangeEnd,
        TimeZoneInfo timeZone,
        IReadOnlyList<WorkingHoursPerDay> workingHours,
        IReadOnlyList<TimeOff> timeOffs,
        IReadOnlyList<Appointment> appointments)
    {
        var workingHoursByDay = workingHours.ToDictionary(x => x.DayOfWeek);
        var builder = new StringBuilder();
        builder.AppendLine(GetPeriodTitle(period, rangeStart, rangeEnd));
        builder.AppendLine();

        var listItems = new List<ScheduleAppointmentListItem>();
        var listIndex = 1;

        for (var date = rangeStart; date <= rangeEnd; date = date.AddDays(1))
        {
            AppendDaySection(
                builder,
                date,
                timeZone,
                workingHoursByDay,
                timeOffs,
                appointments,
                listItems,
                ref listIndex);
        }

        if (listItems.Count == 0)
        {
            builder.AppendLine(MasterScheduleMessages.NoAppointmentsInPeriod);
        }
        else
        {
            builder.AppendLine();
            builder.Append(MasterScheduleMessages.EnterAppointmentNumber);
        }

        var fullText = builder.ToString().TrimEnd();
        var parts = TelegramMessageSplitter.Split(fullText);

        return new MasterScheduleViewResult
        {
            MessageParts = parts,
            Appointments = listItems
        };
    }

    public static string FormatAppointmentDetails(Appointment appointment, TimeZoneInfo timeZone)
    {
        var start = MasterTimeZoneHelper.ToLocal(appointment.StartDateTime, timeZone);
        var end = MasterTimeZoneHelper.ToLocal(appointment.EndDateTime, timeZone);
        var date = DateOnly.FromDateTime(start);

        return MasterScheduleMessages.AppointmentDetails(
            appointment.Service.ServiceName,
            date,
            TimeOnly.FromDateTime(start),
            TimeOnly.FromDateTime(end),
            appointment.Client.ClientName,
            appointment.Client.ClientPhone,
            ScheduleDisplayHelper.FormatAppointmentStatus(appointment.Status, titleCase: true),
            appointment.PriceAtBooking,
            appointment.PrepaymentAmount);
    }

    private static void AppendDaySection(
        StringBuilder builder,
        DateOnly date,
        TimeZoneInfo timeZone,
        IReadOnlyDictionary<Weekday, WorkingHoursPerDay> workingHoursByDay,
        IReadOnlyList<TimeOff> timeOffs,
        IReadOnlyList<Appointment> appointments,
        List<ScheduleAppointmentListItem> listItems,
        ref int listIndex)
    {
        var weekday = (Weekday)(int)date.DayOfWeek;
        var dayLabel = ScheduleDisplayHelper.GetDayLabel(weekday);
        builder.AppendLine($"{date:dd.MM.yyyy} ({dayLabel})");

        var (dayStartUtc, dayEndUtc) = MasterTimeZoneHelper.GetDayBoundsUtc(date, timeZone);

        if (ScheduleDisplayHelper.IsFullDayOff(timeOffs, dayStartUtc, dayEndUtc))
        {
            builder.AppendLine(MasterScheduleMessages.DayOff);
            builder.AppendLine();
            return;
        }

        if (workingHoursByDay.TryGetValue(weekday, out var hours))
        {
            builder.AppendLine(MasterScheduleMessages.WorkingHoursLine(hours.WorkStartTime, hours.WorkEndTime));
        }
        else
        {
            builder.AppendLine(MasterScheduleMessages.NotWorking);
        }

        var dayTimeOffs = timeOffs
            .Where(x => x.StartDateTime < dayEndUtc && x.EndDateTime > dayStartUtc && !ScheduleDisplayHelper.IsFullDayOffBlock(x, dayStartUtc, dayEndUtc))
            .ToList();

        foreach (var block in dayTimeOffs)
        {
            var start = MasterTimeZoneHelper.ToLocal(block.StartDateTime, timeZone);
            var end = MasterTimeZoneHelper.ToLocal(block.EndDateTime, timeZone);
            builder.AppendLine(MasterScheduleMessages.TimeOffBlockLine(
                TimeOnly.FromDateTime(start),
                TimeOnly.FromDateTime(end)));
        }

        var dayAppointments = appointments
            .Where(x =>
            {
                var startLocal = MasterTimeZoneHelper.ToLocal(x.StartDateTime, timeZone);
                return DateOnly.FromDateTime(startLocal) == date;
            })
            .OrderBy(x => x.StartDateTime)
            .ToList();

        if (dayAppointments.Count == 0)
        {
            builder.AppendLine(MasterScheduleMessages.NoAppointmentsOnDay);
        }
        else
        {
            foreach (var appointment in dayAppointments)
            {
                var start = MasterTimeZoneHelper.ToLocal(appointment.StartDateTime, timeZone);
                var end = MasterTimeZoneHelper.ToLocal(appointment.EndDateTime, timeZone);

                builder.AppendLine(
                    $"{listIndex}) {start:HH:mm}–{end:HH:mm} — {appointment.Service.ServiceName} — {appointment.Client.ClientName}");

                listItems.Add(new ScheduleAppointmentListItem
                {
                    ListIndex = listIndex,
                    AppointmentId = appointment.Id
                });

                listIndex++;
            }
        }

        builder.AppendLine();
    }

    private static string GetPeriodTitle(ScheduleViewPeriod period, DateOnly start, DateOnly end) =>
        period switch
        {
            ScheduleViewPeriod.Tomorrow => MasterScheduleMessages.PeriodTitleTomorrow(start),
            ScheduleViewPeriod.ThreeDays => MasterScheduleMessages.PeriodTitleThreeDays(start, end),
            ScheduleViewPeriod.Week => MasterScheduleMessages.PeriodTitleWeek(start, end),
            ScheduleViewPeriod.Month => MasterScheduleMessages.PeriodTitleMonth(start, end),
            _ => MasterScheduleMessages.DefaultPeriodTitle
        };
}
