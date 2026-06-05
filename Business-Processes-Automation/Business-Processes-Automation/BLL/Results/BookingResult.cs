using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.BLL.Results;

public sealed class BookingResult
{
    public bool Success { get; init; }

    public string? ErrorMessage { get; init; }

    public Appointment? Appointment { get; init; }

    public static BookingResult Ok(Appointment appointment) =>
        new() { Success = true, Appointment = appointment };

    public static BookingResult Fail(string message) =>
        new() { Success = false, ErrorMessage = message };
}
