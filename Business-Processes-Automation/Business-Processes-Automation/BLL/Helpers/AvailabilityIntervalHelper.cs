namespace Business_Processes_Automation.BLL.Helpers;

internal readonly record struct TimeRange(DateTime Start, DateTime End);

internal static class AvailabilityIntervalHelper
{
    public static IReadOnlyList<TimeRange> Subtract(
        IReadOnlyList<TimeRange> freeRanges,
        TimeRange busy)
    {
        var result = new List<TimeRange>();

        foreach (var free in freeRanges)
        {
            if (busy.End <= free.Start || busy.Start >= free.End)
            {
                result.Add(free);
                continue;
            }

            if (busy.Start > free.Start)
            {
                result.Add(new TimeRange(free.Start, busy.Start));
            }

            if (busy.End < free.End)
            {
                result.Add(new TimeRange(busy.End, free.End));
            }
        }

        return result;
    }

    public static IReadOnlyList<TimeRange> SubtractAll(
        IReadOnlyList<TimeRange> freeRanges,
        IEnumerable<TimeRange> busyRanges)
    {
        var current = freeRanges;

        foreach (var busy in busyRanges.OrderBy(x => x.Start))
        {
            current = Subtract(current, busy);
        }

        return current;
    }
}
