namespace Business_Processes_Automation.DAL.Entities;

public class TimeOff : BaseEntity
{
    public int MasterId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }

    // Navigation property
    public Master Master { get; set; } = null!;
}
