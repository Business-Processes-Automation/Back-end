using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.BLL.Helpers;

public static class ServiceOccupiedTimeHelper
{
    public static int ResolveTotalOccupiedMinutes(Service service) =>
        service.TotalOccupiedMinutes > 0
            ? service.TotalOccupiedMinutes
            : service.PreparationBeforeInMinutes
              + service.DurationInMinutes
              + service.PreparationAfterInMinutes;

    public static DateTime GetOccupiedEndUtc(DateTime startUtc, Service service) =>
        startUtc.AddMinutes(ResolveTotalOccupiedMinutes(service));

    public static DateTime GetBusyEndUtc(Appointment appointment, int bufferMinutes) =>
        GetOccupiedEndUtc(appointment.StartDateTime, appointment.Service).AddMinutes(bufferMinutes);

    public static bool OverlapsBusyBlock(
        DateTime slotStartUtc,
        DateTime slotEndUtc,
        Appointment appointment,
        int bufferMinutes)
    {
        var busyStart = appointment.StartDateTime;
        var busyEnd = GetBusyEndUtc(appointment, bufferMinutes);
        return slotStartUtc < busyEnd && slotEndUtc > busyStart;
    }
}
