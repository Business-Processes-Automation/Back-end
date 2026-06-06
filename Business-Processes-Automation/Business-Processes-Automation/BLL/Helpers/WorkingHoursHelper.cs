using Business_Processes_Automation.BLL.DTOs.Schedule;
using Business_Processes_Automation.BLL.Localization;
using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.Helpers;

public static class WorkingHoursHelper
{
    public const int MinWorkingDayMinutes = 30;

    private static readonly Weekday[] WeekDisplayOrder =
    [
        Weekday.Monday,
        Weekday.Tuesday,
        Weekday.Wednesday,
        Weekday.Thursday,
        Weekday.Friday,
        Weekday.Saturday,
        Weekday.Sunday
    ];

    public static IReadOnlyList<Weekday> GetWeekDisplayOrder() => WeekDisplayOrder;

    public static IReadOnlyList<WorkingHoursDayResponseDTO> MapWeek(
        IReadOnlyList<WorkingHoursPerDay> workingHours)
    {
        var byDay = workingHours.ToDictionary(x => x.DayOfWeek);

        return WeekDisplayOrder
            .Select(day => byDay.TryGetValue(day, out var entry)
                ? MapWorkingDay(entry)
                : MapDayOff(day))
            .ToList();
    }

    public static WorkingHoursDayResponseDTO MapWorkingDay(WorkingHoursPerDay entry) =>
        new()
        {
            DayOfWeek = entry.DayOfWeek,
            IsWorking = true,
            WorkStartTime = FormatTime(entry.WorkStartTime),
            WorkEndTime = FormatTime(entry.WorkEndTime)
        };

    public static WorkingHoursDayResponseDTO MapDayOff(Weekday day) =>
        new()
        {
            DayOfWeek = day,
            IsWorking = false,
            WorkStartTime = null,
            WorkEndTime = null
        };

    public static string? ValidateWeekDays(IReadOnlyList<WorkingHoursDayRequestDTO> days)
    {
        if (days.Count != WeekDisplayOrder.Length)
        {
            return ScheduleSettingsMessages.WorkingHoursMustContainSevenDays;
        }

        var uniqueDays = days.Select(x => x.DayOfWeek).Distinct().ToList();
        if (uniqueDays.Count != WeekDisplayOrder.Length)
        {
            return ScheduleSettingsMessages.WorkingHoursDuplicateDay;
        }

        if (uniqueDays.Except(WeekDisplayOrder).Any())
        {
            return ScheduleSettingsMessages.WorkingHoursInvalidDay;
        }

        foreach (var day in days)
        {
            var error = ValidateDayRequest(day);
            if (error is not null)
            {
                return error;
            }
        }

        return null;
    }

    public static string? ValidateDayRequest(UpdateWorkingHoursDayRequestDTO request)
    {
        if (!request.IsWorking)
        {
            return null;
        }

        return ValidateWorkingTimes(request.WorkStartTime, request.WorkEndTime);
    }

    public static string? ValidateDayRequest(WorkingHoursDayRequestDTO request)
    {
        if (!request.IsWorking)
        {
            return null;
        }

        return ValidateWorkingTimes(request.WorkStartTime, request.WorkEndTime);
    }

    public static string? ValidateWorkingTimes(string? workStartTime, string? workEndTime)
    {
        if (string.IsNullOrWhiteSpace(workStartTime) || string.IsNullOrWhiteSpace(workEndTime))
        {
            return ScheduleSettingsMessages.WorkingDayTimesRequired;
        }

        if (!TryParseTime(workStartTime, out var start) || !TryParseTime(workEndTime, out var end))
        {
            return ScheduleSettingsMessages.InvalidWorkTimeFormat;
        }

        if (end <= start)
        {
            return ScheduleSettingsMessages.EndTimeMustBeAfterStart;
        }

        if ((end - start).TotalMinutes < MinWorkingDayMinutes)
        {
            return ScheduleSettingsMessages.WorkingDayTooShort;
        }

        return null;
    }

    public static bool TryParseTime(string value, out TimeOnly time) =>
        TimeOnly.TryParseExact(
            value.Trim(),
            ["HH:mm", "H:mm"],
            null,
            System.Globalization.DateTimeStyles.None,
            out time);

    public static string FormatTime(TimeOnly time) => time.ToString("HH:mm");
}
