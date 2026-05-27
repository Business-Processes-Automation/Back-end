using System.Text;
using Business_Processes_Automation.BLL.DTOs.Schedule;
using Business_Processes_Automation.BLL.Enums;
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
        builder.AppendLine($"{date:dd.MM.yyyy} — {service.ServiceName}");
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
            builder.Append("На цей день немає вільних слотів.");
        }
        else
        {
            builder.Append("Оберіть час для запису:");
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
        builder.AppendLine("Послуги для запису:");
        for (var i = 0; i < services.Count; i++)
        {
            var service = services[i];
            builder.AppendLine(
                $"{i + 1}. {service.ServiceName} — {service.DurationInMinutes} хв, {service.Price:0} грн");
        }

        builder.AppendLine();
        builder.Append("Введіть номер послуги, щоб побачити вільні слоти.");

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
        builder.AppendLine($"Послуга: {service.ServiceName} ({service.DurationInMinutes} хв)");
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
                builder.AppendLine("Вільних слотів немає");
            }
            else
            {
                builder.AppendLine("Вільні слоти:");
                foreach (var slot in daySlots)
                {
                    builder.AppendLine($"{slot.ListIndex}) {slot.StartTime:HH:mm}");
                }
            }

            builder.AppendLine();
        }

        if (freeSlots.Count == 0)
        {
            builder.AppendLine("Немає вільних слотів для цієї послуги в обраному періоді.");
        }
        else
        {
            builder.Append("Введіть номер слота для запису.");
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

        return "Підтвердіть запис:\n\n" +
               $"Послуга: {service.ServiceName}\n" +
               $"Дата: {slot.LocalDate:dd.MM.yyyy}\n" +
               $"Час: {slot.StartTime:HH:mm} – {endLocal:HH:mm}\n" +
               $"Тривалість: {service.DurationInMinutes} хв\n" +
               $"Ціна: {price:0.##} грн\n" +
               $"Передоплата: {prepayment:0.##} грн";
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
        builder.AppendLine($"{date:dd.MM.yyyy} ({FormatDayLabel(weekday)})");

        var (dayStartUtc, dayEndUtc) = MasterTimeZoneHelper.GetDayBoundsUtc(date, timeZone);

        if (IsFullDayOff(timeOffs, dayStartUtc, dayEndUtc))
        {
            builder.AppendLine("Вихідний");
            return;
        }

        if (workingHoursByDay.TryGetValue(weekday, out var hours))
        {
            builder.AppendLine($"Робочий час: {hours.WorkStartTime:HH:mm} – {hours.WorkEndTime:HH:mm}");
        }
        else
        {
            builder.AppendLine("Майстер не працює");
            return;
        }

        var dayTimeOffs = timeOffs
            .Where(x => x.StartDateTime < dayEndUtc && x.EndDateTime > dayStartUtc
                        && !IsFullDayOffBlock(x, dayStartUtc, dayEndUtc))
            .ToList();

        foreach (var block in dayTimeOffs)
        {
            var start = MasterTimeZoneHelper.ToLocal(block.StartDateTime, timeZone);
            var end = MasterTimeZoneHelper.ToLocal(block.EndDateTime, timeZone);
            builder.AppendLine($"Недоступно: {start:HH:mm} – {end:HH:mm}");
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
            builder.AppendLine($"Зайнято: {start:HH:mm}–{end:HH:mm}");
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

    private static bool IsFullDayOff(IReadOnlyList<TimeOff> timeOffs, DateTime dayStartUtc, DateTime dayEndUtc) =>
        timeOffs.Any(x => IsFullDayOffBlock(x, dayStartUtc, dayEndUtc));

    private static bool IsFullDayOffBlock(TimeOff block, DateTime dayStartUtc, DateTime dayEndUtc) =>
        block.StartDateTime <= dayStartUtc && block.EndDateTime >= dayEndUtc;

    private static string GetPeriodTitle(ScheduleViewPeriod period, DateOnly start, DateOnly end) =>
        period switch
        {
            ScheduleViewPeriod.Tomorrow => $"Запис на завтра ({start:dd.MM.yyyy})",
            ScheduleViewPeriod.ThreeDays => $"Запис на 3 дні ({start:dd.MM} – {end:dd.MM})",
            ScheduleViewPeriod.Week => $"Запис на тиждень ({start:dd.MM} – {end:dd.MM})",
            ScheduleViewPeriod.Month => $"Запис на місяць ({start:dd.MM} – {end:dd.MM})",
            _ => "Запис"
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
}
