using Business_Processes_Automation.BLL.DTOs.Service;
using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.Helpers;

public static class ScheduleDisplayHelper
{
    public static string GetDayLabel(Weekday day) => day switch
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

    public static bool TryParseDayLabel(string text, out Weekday day)
    {
        day = default;
        foreach (Weekday value in Enum.GetValues<Weekday>())
        {
            if (text == GetDayLabel(value))
            {
                day = value;
                return true;
            }
        }

        return false;
    }

    public static string FormatServiceLine(Service service, int index) =>
        FormatServiceLine(service.ServiceName, service.DurationInMinutes, service.Price, index);

    public static string FormatServiceLine(ServiceResponseDTO service, int index) =>
        FormatServiceLine(service.ServiceName, service.DurationInMinutes, service.Price, index);

    private static string FormatServiceLine(string serviceName, int durationMinutes, decimal price, int index) =>
        $"{index}. {serviceName} — {durationMinutes} хв, {price:0} грн";

    public static string FormatAppointmentStatus(AppointmentStatus status, bool titleCase = false)
    {
        var label = status switch
        {
            AppointmentStatus.Planned => "заплановано",
            AppointmentStatus.Completed => "завершено",
            AppointmentStatus.Cancelled => "скасовано",
            AppointmentStatus.Rescheduled => "перенесено",
            AppointmentStatus.NoShow => "не з'явився",
            _ => status.ToString()
        };

        return titleCase ? char.ToUpper(label[0]) + label[1..] : label;
    }

    public static AppointmentStatus ResolveDisplayStatus(Appointment appointment, DateTime nowUtc) =>
        appointment.EndDateTime < nowUtc
        && appointment.Status is AppointmentStatus.Planned or AppointmentStatus.Rescheduled
            ? AppointmentStatus.Completed
            : appointment.Status;

    public static bool IsUpcomingAppointment(Appointment appointment, DateTime nowUtc) =>
        appointment.EndDateTime >= nowUtc
        && appointment.Status is AppointmentStatus.Planned or AppointmentStatus.Rescheduled;

    public static bool IsFullDayOff(IReadOnlyList<TimeOff> timeOffs, DateTime dayStartUtc, DateTime dayEndUtc) =>
        timeOffs.Any(x => IsFullDayOffBlock(x, dayStartUtc, dayEndUtc));

    public static bool IsFullDayOffBlock(TimeOff block, DateTime dayStartUtc, DateTime dayEndUtc) =>
        block.StartDateTime <= dayStartUtc && block.EndDateTime >= dayEndUtc;
}
