using System.Text;
using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.Helpers;

public static class ClientAppointmentsTextFormatter
{
    public static string FormatMyAppointments(
        IReadOnlyList<Appointment> appointments,
        string masterDisplayName,
        string masterPhoneNumber,
        string masterTimeZoneId,
        TimeZoneInfo timeZone)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Мої записи до майстра");
        builder.AppendLine();
        builder.AppendLine($"Майстер: {masterDisplayName}");
        builder.AppendLine($"Телефон: {masterPhoneNumber}");
        builder.AppendLine($"Часовий пояс: {masterTimeZoneId}");
        builder.AppendLine();

        if (appointments.Count == 0)
        {
            builder.AppendLine("У вас поки немає записів у цього майстра.");
            return builder.ToString().TrimEnd();
        }

        var nowUtc = DateTime.UtcNow;
        var upcoming = appointments
            .Where(x => IsUpcoming(x, nowUtc))
            .OrderBy(x => x.StartDateTime)
            .ToList();
        var past = appointments
            .Where(x => !IsUpcoming(x, nowUtc))
            .OrderByDescending(x => x.StartDateTime)
            .ToList();

        AppendSection(builder, "Майбутні:", upcoming, timeZone, nowUtc);
        AppendSection(builder, "Минулі:", past, timeZone, nowUtc);

        return builder.ToString().TrimEnd();
    }

    private static void AppendSection(
        StringBuilder builder,
        string title,
        IReadOnlyList<Appointment> items,
        TimeZoneInfo timeZone,
        DateTime nowUtc)
    {
        builder.AppendLine(title);

        if (items.Count == 0)
        {
            builder.AppendLine("—");
            builder.AppendLine();
            return;
        }

        for (var i = 0; i < items.Count; i++)
        {
            builder.AppendLine(FormatLine(i + 1, items[i], timeZone, nowUtc));
        }

        builder.AppendLine();
    }

    private static string FormatLine(int index, Appointment appointment, TimeZoneInfo timeZone, DateTime nowUtc)
    {
        var start = MasterTimeZoneHelper.ToLocal(appointment.StartDateTime, timeZone);
        var end = MasterTimeZoneHelper.ToLocal(appointment.EndDateTime, timeZone);

        return $"{index}. {start:dd.MM.yyyy} {start:HH:mm}–{end:HH:mm} — {appointment.Service.ServiceName} ({FormatStatus(ResolveStatus(appointment, nowUtc))})";
    }

    private static bool IsUpcoming(Appointment appointment, DateTime nowUtc) =>
        appointment.EndDateTime >= nowUtc
        && appointment.Status is AppointmentStatus.Planned or AppointmentStatus.Rescheduled;

    private static AppointmentStatus ResolveStatus(Appointment appointment, DateTime nowUtc) =>
        appointment.EndDateTime < nowUtc
        && appointment.Status is AppointmentStatus.Planned or AppointmentStatus.Rescheduled
            ? AppointmentStatus.Completed
            : appointment.Status;

    private static string FormatStatus(AppointmentStatus status) => status switch
    {
        AppointmentStatus.Planned => "заплановано",
        AppointmentStatus.Completed => "завершено",
        AppointmentStatus.Cancelled => "скасовано",
        AppointmentStatus.Rescheduled => "перенесено",
        AppointmentStatus.NoShow => "не з'явився",
        _ => status.ToString()
    };
}
