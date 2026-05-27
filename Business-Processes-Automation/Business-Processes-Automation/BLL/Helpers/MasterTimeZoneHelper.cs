using System.Runtime.InteropServices;

namespace Business_Processes_Automation.BLL.Helpers;

public static class MasterTimeZoneHelper
{
    // NOTE: On Windows, IANA zone IDs like "Europe/Kyiv" are often not available.
    // We default to the Windows zone ID for Kyiv when running on Windows.
    private const string DefaultIanaTimeZoneId = "Europe/Kyiv";
    private const string DefaultWindowsTimeZoneId = "FLE Standard Time";

    public static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            return ResolveDefaultTimeZone();
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId.Trim());
        }
        catch (TimeZoneNotFoundException)
        {
            return ResolveDefaultTimeZone();
        }
        catch (InvalidTimeZoneException)
        {
            return ResolveDefaultTimeZone();
        }
    }

    private static TimeZoneInfo ResolveDefaultTimeZone()
    {
        var id = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? DefaultWindowsTimeZoneId
            : DefaultIanaTimeZoneId;

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch
        {
            return TimeZoneInfo.Local;
        }
    }

    public static DateTime ToUtc(DateTime local, TimeZoneInfo timeZone)
    {
        var unspecified = DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(unspecified, timeZone);
    }

    public static DateTime ToLocal(DateTime utc, TimeZoneInfo timeZone)
    {
        var utcNormalized = utc.Kind == DateTimeKind.Utc
            ? utc
            : DateTime.SpecifyKind(utc, DateTimeKind.Utc);

        return TimeZoneInfo.ConvertTimeFromUtc(utcNormalized, timeZone);
    }

    public static DateOnly ToLocalDate(DateTime utc, TimeZoneInfo timeZone)
    {
        var local = ToLocal(utc, timeZone);
        return DateOnly.FromDateTime(local);
    }

    public static (DateTime StartUtc, DateTime EndUtc) GetDayBoundsUtc(DateOnly localDate, TimeZoneInfo timeZone)
    {
        var startLocal = localDate.ToDateTime(TimeOnly.MinValue);
        var endLocal = localDate.ToDateTime(new TimeOnly(23, 59, 59, 999));

        return (ToUtc(startLocal, timeZone), ToUtc(endLocal, timeZone));
    }
}
