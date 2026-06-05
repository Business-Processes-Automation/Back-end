using System.Text;
using Business_Processes_Automation.BLL.Localization;
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
        builder.AppendLine(ClientAppointmentsMessages.Title);
        builder.AppendLine();
        builder.AppendLine(ClientAppointmentsMessages.MasterLine(masterDisplayName));
        builder.AppendLine(ClientAppointmentsMessages.PhoneLine(masterPhoneNumber));
        builder.AppendLine(ClientAppointmentsMessages.TimeZoneLine(masterTimeZoneId));
        builder.AppendLine();

        if (appointments.Count == 0)
        {
            builder.AppendLine(ClientAppointmentsMessages.Empty);
            return builder.ToString().TrimEnd();
        }

        var nowUtc = DateTime.UtcNow;
        var upcoming = appointments
            .Where(x => ScheduleDisplayHelper.IsUpcomingAppointment(x, nowUtc))
            .OrderBy(x => x.StartDateTime)
            .ToList();
        var past = appointments
            .Where(x => !ScheduleDisplayHelper.IsUpcomingAppointment(x, nowUtc))
            .OrderByDescending(x => x.StartDateTime)
            .ToList();

        AppendSection(builder, ClientAppointmentsMessages.UpcomingSection, upcoming, timeZone, nowUtc);
        AppendSection(builder, ClientAppointmentsMessages.PastSection, past, timeZone, nowUtc);

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
            builder.AppendLine(ClientAppointmentsMessages.EmptySection);
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
        var status = ScheduleDisplayHelper.ResolveDisplayStatus(appointment, nowUtc);

        return $"{index}. {start:dd.MM.yyyy} {start:HH:mm}–{end:HH:mm} — {appointment.Service.ServiceName} ({ScheduleDisplayHelper.FormatAppointmentStatus(status)})";
    }
}
