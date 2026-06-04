using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.BLL.Services;

public sealed class ScheduleSettingsResult
{
    public bool Success { get; init; }

    public string? ErrorMessage { get; init; }

    public WorkingHoursPerDay? WorkingHours { get; init; }

    public MasterAppointmentSetting? Setting { get; init; }

    public TimeOff? TimeOff { get; init; }

    public static ScheduleSettingsResult Ok() =>
        new() { Success = true };

    public static ScheduleSettingsResult Ok(WorkingHoursPerDay workingHours) =>
        new() { Success = true, WorkingHours = workingHours };

    public static ScheduleSettingsResult Ok(MasterAppointmentSetting setting) =>
        new() { Success = true, Setting = setting };

    public static ScheduleSettingsResult Ok(TimeOff timeOff) =>
        new() { Success = true, TimeOff = timeOff };

    public static ScheduleSettingsResult Fail(string message) =>
        new() { Success = false, ErrorMessage = message };
}
