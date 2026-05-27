namespace Business_Processes_Automation.BLL.DTOs.Schedule;

public sealed class FreeSlot
{
    public int ListIndex { get; init; }

    public DateOnly LocalDate { get; init; }

    public TimeOnly StartTime { get; init; }

    public DateTime StartUtc { get; init; }

    public DateTime EndUtc { get; init; }
}
