using Business_Processes_Automation.BLL.Localization;

namespace Business_Processes_Automation.BLL.Helpers;

public static class AppointmentBookingErrorMapper
{
    public static string ToClientBookingMessage(string? errorMessage) =>
        errorMessage switch
        {
            MasterAppointmentMessages.SlotAlreadyTaken => ClientBookingMessages.SlotAlreadyTaken,
            MasterAppointmentMessages.SlotUnavailable => ClientBookingMessages.SlotUnavailable,
            MasterAppointmentMessages.MinBookingNoticeViolation => ClientBookingMessages.SlotUnavailable,
            MasterAppointmentMessages.MaxBookingDaysExceeded => ClientBookingMessages.SlotUnavailable,
            MasterAppointmentMessages.OutsideWorkingHours => ClientBookingMessages.SlotUnavailable,
            MasterAppointmentMessages.NonWorkingDay => ClientBookingMessages.SlotUnavailable,
            MasterAppointmentMessages.FullDayOff => ClientBookingMessages.SlotUnavailable,
            MasterAppointmentMessages.ServiceNotFound => ClientBookingMessages.SlotUnavailable,
            null or "" => ClientBookingMessages.SlotUnavailable,
            _ => errorMessage
        };
}
