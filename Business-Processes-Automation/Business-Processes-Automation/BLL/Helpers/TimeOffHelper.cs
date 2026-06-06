using Business_Processes_Automation.BLL.DTOs.Schedule;
using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.BLL.Helpers;

public static class TimeOffHelper
{
    public static (DateTime StartLocal, DateTime EndLocal) NormalizeLocalRange(
        DateTime startLocal,
        DateTime endLocal,
        bool isFullDay)
    {
        if (!isFullDay)
        {
            return (startLocal, endLocal);
        }

        var date = DateOnly.FromDateTime(startLocal);
        var dayStart = date.ToDateTime(TimeOnly.MinValue);
        var dayEnd = date.ToDateTime(new TimeOnly(23, 59, 59));

        return (dayStart, dayEnd);
    }

    public static bool IsFullDayBlock(DateTime startLocal, DateTime endLocal)
    {
        if (DateOnly.FromDateTime(startLocal) != DateOnly.FromDateTime(endLocal))
        {
            return false;
        }

        return startLocal.TimeOfDay == TimeSpan.Zero
               && endLocal.TimeOfDay >= TimeSpan.FromHours(23);
    }

    public static TimeOffResponseDTO MapToDto(TimeOff timeOff, TimeZoneInfo timeZone)
    {
        var startLocal = MasterTimeZoneHelper.ToLocal(timeOff.StartDateTime, timeZone);
        var endLocal = MasterTimeZoneHelper.ToLocal(timeOff.EndDateTime, timeZone);

        return new TimeOffResponseDTO
        {
            Id = timeOff.Id,
            StartLocal = startLocal,
            EndLocal = endLocal,
            IsFullDay = IsFullDayBlock(startLocal, endLocal)
        };
    }
}
