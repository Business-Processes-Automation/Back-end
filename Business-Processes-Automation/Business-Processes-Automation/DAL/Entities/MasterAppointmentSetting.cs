namespace Business_Processes_Automation.DAL.Entities;

public class MasterAppointmentSetting : BaseEntity
{
    public int MasterId { get; set; }
    public int MinBookingNoticeMinutes { get; set; }
    public int MaxBookingDaysAhead { get; set; } = 30;
    public int CancellationPolicyHours { get; set; } = 24;

    // Navigation property
    public Master Master { get; set; } = null!;
}
