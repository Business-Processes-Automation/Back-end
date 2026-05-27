namespace Business_Processes_Automation.BLL.SessionDrafts;

public class TimeOffDraft
{
    public DateOnly? LocalDate { get; set; }

    public bool? IsFullDay { get; set; }

    public TimeOnly? LocalStart { get; set; }

    public TimeOnly? LocalEnd { get; set; }

    public List<int>? ListedTimeOffIds { get; set; }
}
