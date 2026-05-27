using Business_Processes_Automation.BLL.DTOs.Schedule;
using Business_Processes_Automation.BLL.Enums;

namespace Business_Processes_Automation.BLL.SessionDrafts;

public class BookingDraft
{
    public ScheduleViewPeriod Period { get; set; }

    public DateOnly RangeStart { get; set; }

    public DateOnly RangeEnd { get; set; }

    public int? ServiceId { get; set; }

    public List<FreeSlot> Slots { get; set; } = [];

    public List<int> ServiceIdsInOrder { get; set; } = [];
}
