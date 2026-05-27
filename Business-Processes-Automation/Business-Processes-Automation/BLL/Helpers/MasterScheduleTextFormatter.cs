using System.Text;
using Business_Processes_Automation.BLL.DTOs.Schedule;
using Business_Processes_Automation.BLL.Enums;
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
            builder.AppendLine("Записів у цьому періоді немає.");
        }
        else
        {
            builder.AppendLine();
            builder.Append("Введіть номер запису, щоб побачити деталі.");
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

        return "Деталі запису\n\n" +
               $"Послуга: {appointment.Service.ServiceName}\n" +
               $"Дата: {date:dd.MM.yyyy}\n" +
               $"Час: {start:HH:mm} – {end:HH:mm}\n" +
               $"Клієнт: {appointment.Client.ClientName}\n" +
               $"Телефон: {appointment.Client.ClientPhone}\n" +
               $"Статус: {FormatStatus(appointment.Status)}\n" +
               $"Ціна: {appointment.PriceAtBooking:0.##} грн\n" +
               $"Передоплата: {appointment.PrepaymentAmount:0.##} грн";
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
        var dayLabel = FormatDayLabel(weekday);
        builder.AppendLine($"{date:dd.MM.yyyy} ({dayLabel})");

        var (dayStartUtc, dayEndUtc) = MasterTimeZoneHelper.GetDayBoundsUtc(date, timeZone);

        if (IsFullDayOff(timeOffs, dayStartUtc, dayEndUtc))
        {
            builder.AppendLine("Вихідний");
            builder.AppendLine();
            return;
        }

        if (workingHoursByDay.TryGetValue(weekday, out var hours))
        {
            builder.AppendLine($"Робочий час: {hours.WorkStartTime:HH:mm} – {hours.WorkEndTime:HH:mm}");
        }
        else
        {
            builder.AppendLine("Не працюю");
        }

        var dayTimeOffs = timeOffs
            .Where(x => x.StartDateTime < dayEndUtc && x.EndDateTime > dayStartUtc && !IsFullDayOffBlock(x, dayStartUtc, dayEndUtc))
            .ToList();

        foreach (var block in dayTimeOffs)
        {
            var start = MasterTimeZoneHelper.ToLocal(block.StartDateTime, timeZone);
            var end = MasterTimeZoneHelper.ToLocal(block.EndDateTime, timeZone);
            builder.AppendLine($"Блок: {start:HH:mm} – {end:HH:mm}");
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
            builder.AppendLine("Записів немає");
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

    private static bool IsFullDayOff(IReadOnlyList<TimeOff> timeOffs, DateTime dayStartUtc, DateTime dayEndUtc) =>
        timeOffs.Any(x => IsFullDayOffBlock(x, dayStartUtc, dayEndUtc));

    private static bool IsFullDayOffBlock(TimeOff block, DateTime dayStartUtc, DateTime dayEndUtc) =>
        block.StartDateTime <= dayStartUtc && block.EndDateTime >= dayEndUtc;

    private static string GetPeriodTitle(ScheduleViewPeriod period, DateOnly start, DateOnly end) =>
        period switch
        {
            ScheduleViewPeriod.Tomorrow => $"Розклад на завтра ({start:dd.MM.yyyy})",
            ScheduleViewPeriod.ThreeDays => $"Розклад на 3 дні ({start:dd.MM} – {end:dd.MM})",
            ScheduleViewPeriod.Week => $"Розклад на тиждень ({start:dd.MM} – {end:dd.MM})",
            ScheduleViewPeriod.Month => $"Розклад на місяць ({start:dd.MM} – {end:dd.MM})",
            _ => "Розклад"
        };

    private static string FormatDayLabel(Weekday day) => day switch
    {
        Weekday.Monday => "Пн",
        Weekday.Tuesday => "Вт",
        Weekday.Wednesday => "Ср",
        Weekday.Thursday => "Чт",
        Weekday.Friday => "Пт",
        Weekday.Saturday => "Сб",
        Weekday.Sunday => "Нд",
        _ => day.ToString()
    };

    private static string FormatStatus(AppointmentStatus status) => status switch
    {
        AppointmentStatus.Planned => "Заплановано",
        AppointmentStatus.Completed => "Завершено",
        AppointmentStatus.Cancelled => "Скасовано",
        AppointmentStatus.Rescheduled => "Перенесено",
        AppointmentStatus.NoShow => "Не з'явився",
        _ => status.ToString()
    };
}
