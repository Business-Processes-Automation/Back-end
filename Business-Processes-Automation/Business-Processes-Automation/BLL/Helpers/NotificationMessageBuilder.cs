using System.Text;
using Business_Processes_Automation.BLL.Localization;
using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.Helpers;

public static class NotificationMessageBuilder
{
    public static string Build(
        ScheduledNotificationKind kind,
        Appointment appointment,
        TimeZoneInfo timeZone,
        Master? master = null,
        DateTime? previousStartLocal = null)
    {
        var service = appointment.Service;
        var slot = GetServiceSlotLocal(appointment, timeZone);

        return kind switch
        {
            ScheduledNotificationKind.BookingConfirmed =>
                BuildBookingConfirmed(slot, service, master),
            ScheduledNotificationKind.Reminder24Hours =>
                BuildReminder24Hours(slot, service, timeZone),
            ScheduledNotificationKind.Reminder1Hour =>
                BuildReminder1Hour(slot, service),
            ScheduledNotificationKind.BookingCancelled =>
                BuildBookingCancelled(slot, service),
            ScheduledNotificationKind.BookingRescheduled =>
                BuildBookingRescheduled(slot, service, previousStartLocal),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }

    private static string BuildBookingConfirmed(
        ServiceSlotLocal slot,
        Service service,
        Master? master)
    {
        var builder = new StringBuilder();
        builder.AppendLine(NotificationMessages.BookingConfirmedTitle);
        builder.AppendLine();
        builder.AppendLine(NotificationMessages.FormatServiceAppointment(
            service.ServiceName,
            slot.Date,
            slot.Start,
            slot.EndDisplay,
            service.DurationInMinutes));

        if (master is not null)
        {
            builder.AppendLine();
            builder.Append(NotificationMessages.FormatMasterLine(
                FormatMasterDisplayName(master),
                master.PhoneNumber));
        }

        return builder.ToString().TrimEnd();
    }

    private static string BuildReminder24Hours(
        ServiceSlotLocal slot,
        Service service,
        TimeZoneInfo timeZone)
    {
        var today = MasterTimeZoneHelper.ToLocalDate(DateTime.UtcNow, timeZone);
        var isTomorrow = slot.Date == today.AddDays(1);

        var builder = new StringBuilder();
        builder.AppendLine(NotificationMessages.Reminder24HoursTitle);
        builder.AppendLine(NotificationMessages.FormatReminder24HoursWhen(slot.Date, slot.Start, isTomorrow));
        builder.AppendLine();
        builder.Append(NotificationMessages.FormatServiceAppointment(
            service.ServiceName,
            slot.Date,
            slot.Start,
            slot.EndDisplay,
            service.DurationInMinutes));

        return builder.ToString().TrimEnd();
    }

    private static string BuildReminder1Hour(ServiceSlotLocal slot, Service service)
    {
        var builder = new StringBuilder();
        builder.AppendLine(NotificationMessages.Reminder1HourTitle);
        builder.AppendLine(NotificationMessages.FormatReminder1HourWhen(slot.Date, slot.Start));
        builder.AppendLine();
        builder.Append(NotificationMessages.FormatServiceAppointment(
            service.ServiceName,
            slot.Date,
            slot.Start,
            slot.EndDisplay,
            service.DurationInMinutes));

        return builder.ToString().TrimEnd();
    }

    private static string BuildBookingCancelled(ServiceSlotLocal slot, Service service)
    {
        var builder = new StringBuilder();
        builder.AppendLine(NotificationMessages.BookingCancelledTitle);
        builder.AppendLine();
        builder.Append(NotificationMessages.FormatServiceAppointment(
            service.ServiceName,
            slot.Date,
            slot.Start,
            slot.EndDisplay,
            service.DurationInMinutes));

        return builder.ToString().TrimEnd();
    }

    private static string BuildBookingRescheduled(
        ServiceSlotLocal slot,
        Service service,
        DateTime? previousStartLocal)
    {
        var builder = new StringBuilder();
        builder.AppendLine(NotificationMessages.BookingRescheduledTitle);
        builder.AppendLine();

        if (previousStartLocal.HasValue)
        {
            var previous = GetServiceSlotLocal(previousStartLocal.Value, service);
            builder.AppendLine(NotificationMessages.FormatPreviousSlot(
                previous.Date,
                previous.Start,
                previous.EndDisplay));
        }

        builder.AppendLine(NotificationMessages.FormatNewSlot(
            slot.Date,
            slot.Start,
            slot.EndDisplay));
        builder.AppendLine();
        builder.Append(NotificationMessages.FormatServiceAppointment(
            service.ServiceName,
            slot.Date,
            slot.Start,
            slot.EndDisplay,
            service.DurationInMinutes));

        return builder.ToString().TrimEnd();
    }

    private static ServiceSlotLocal GetServiceSlotLocal(Appointment appointment, TimeZoneInfo timeZone)
    {
        var startLocal = MasterTimeZoneHelper.ToLocal(appointment.StartDateTime, timeZone);
        return GetServiceSlotLocal(startLocal, appointment.Service);
    }

    private static ServiceSlotLocal GetServiceSlotLocal(DateTime startLocal, Service service)
    {
        var date = DateOnly.FromDateTime(startLocal);
        var start = TimeOnly.FromDateTime(startLocal);
        var endDisplay = startLocal.AddMinutes(service.DurationInMinutes);

        return new ServiceSlotLocal(date, start, endDisplay);
    }

    private static string FormatMasterDisplayName(Master master) =>
        $"{master.FirstName} {master.LastName}".Trim();

    private readonly record struct ServiceSlotLocal(
        DateOnly Date,
        TimeOnly Start,
        DateTime EndDisplay);
}
