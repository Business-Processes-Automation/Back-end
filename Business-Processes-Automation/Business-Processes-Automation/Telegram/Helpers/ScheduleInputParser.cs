using System.Globalization;

namespace Business_Processes_Automation.Telegram.Helpers;

public static class ScheduleInputParser
{
    private static readonly string[] TimeFormats = ["H:mm", "HH:mm", "H.mm", "HH.mm"];

    public static bool TryParseTime(string text, out TimeOnly time)
    {
        time = default;
        var normalized = text.Trim().Replace('.', ':');

        return TimeOnly.TryParseExact(
            normalized,
            TimeFormats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out time);
    }

    public static bool TryParseDate(string text, DateOnly today, out DateOnly date)
    {
        date = default;
        var normalized = text.Trim();

        if (normalized.Equals("сьогодні", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("сегодня", StringComparison.OrdinalIgnoreCase))
        {
            date = today;
            return true;
        }

        if (normalized.Equals("завтра", StringComparison.OrdinalIgnoreCase))
        {
            date = today.AddDays(1);
            return true;
        }

        if (DateOnly.TryParseExact(
                normalized,
                "dd.MM.yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date))
        {
            return true;
        }

        if (DateOnly.TryParseExact(
                normalized,
                "dd.MM",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date))
        {
            date = new DateOnly(today.Year, date.Month, date.Day);
            return true;
        }

        return false;
    }

    public static bool TryParseBufferMinutes(string text, out int minutes)
    {
        minutes = default;

        if (!int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out minutes))
        {
            return false;
        }

        return minutes is >= 0 and <= 480;
    }

    public static bool TryParseListIndex(string text, int maxIndex, out int index)
    {
        index = default;

        if (!int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out index))
        {
            return false;
        }

        return index >= 1 && index <= maxIndex;
    }
}
