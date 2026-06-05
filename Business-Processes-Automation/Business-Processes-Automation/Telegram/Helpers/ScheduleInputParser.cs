using System.Globalization;

namespace Business_Processes_Automation.Telegram.Helpers;

public static class ScheduleInputParser
{
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
