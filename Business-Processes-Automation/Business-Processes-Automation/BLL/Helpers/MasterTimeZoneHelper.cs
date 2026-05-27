namespace Business_Processes_Automation.BLL.Helpers;

public static class MasterTimeZoneHelper
{
    public const string DefaultTimeZoneId = "Europe/Kyiv";

    public static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            return TimeZoneInfo.FindSystemTimeZoneById(DefaultTimeZoneId);
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId.Trim());
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById(DefaultTimeZoneId);
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById(DefaultTimeZoneId);
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
