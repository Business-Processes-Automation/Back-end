using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.BLL.Helpers;

public static class AppointmentMapper
{
    public static AppointmentResponseDTO MapToResponse(Appointment appointment, TimeZoneInfo timeZone) =>
        MapCore<AppointmentResponseDTO>(appointment, timeZone);

    public static AppointmentDetailsResponseDTO MapToDetails(Appointment appointment, TimeZoneInfo timeZone) =>
        MapCore<AppointmentDetailsResponseDTO>(appointment, timeZone);

    private static T MapCore<T>(Appointment appointment, TimeZoneInfo timeZone)
        where T : AppointmentResponseDTO, new()
    {
        var displayStatus = ScheduleDisplayHelper.ResolveDisplayStatus(appointment, DateTime.UtcNow);

        return new T
        {
            Id = appointment.Id,
            ServiceId = appointment.ServiceId,
            ServiceName = appointment.Service.ServiceName,
            DurationInMinutes = appointment.Service.DurationInMinutes,
            TotalOccupiedMinutes = ServiceOccupiedTimeHelper.ResolveTotalOccupiedMinutes(appointment.Service),
            ClientId = appointment.ClientId,
            ClientName = appointment.Client.ClientName,
            ClientPhone = appointment.Client.ClientPhone,
            StartLocal = MasterTimeZoneHelper.ToLocal(appointment.StartDateTime, timeZone),
            EndLocal = MasterTimeZoneHelper.ToLocal(appointment.EndDateTime, timeZone),
            Status = appointment.Status,
            DisplayStatus = displayStatus,
            RescheduleCount = appointment.RescheduleCount,
            Notes = appointment.Notes,
            PriceAtBooking = appointment.PriceAtBooking,
            PrepaymentAmount = appointment.PrepaymentAmount
        };
    }
}
