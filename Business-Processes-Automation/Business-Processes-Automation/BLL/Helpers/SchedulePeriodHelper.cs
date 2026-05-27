using Business_Processes_Automation.BLL.Enums;

namespace Business_Processes_Automation.BLL.Helpers;

public static class SchedulePeriodHelper
{
    public static (DateOnly Start, DateOnly End) GetDateRange(ScheduleViewPeriod period, DateOnly today) =>
        period switch
        {
            ScheduleViewPeriod.Tomorrow => (today.AddDays(1), today.AddDays(1)),
            ScheduleViewPeriod.ThreeDays => (today.AddDays(1), today.AddDays(3)),
            ScheduleViewPeriod.Week => (today, today.AddDays(6)),
            ScheduleViewPeriod.Month => (today, today.AddDays(29)),
            _ => (today, today)
        };
}
