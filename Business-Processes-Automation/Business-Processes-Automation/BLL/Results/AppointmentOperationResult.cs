using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.BLL.Results;

public sealed class AppointmentOperationResult
{
    public bool Success { get; init; }

    public string? ErrorMessage { get; init; }

    public Appointment? Appointment { get; init; }

    public static AppointmentOperationResult Ok(Appointment appointment) =>
        new() { Success = true, Appointment = appointment };

    public static AppointmentOperationResult Fail(string message) =>
        new() { Success = false, ErrorMessage = message };
}
