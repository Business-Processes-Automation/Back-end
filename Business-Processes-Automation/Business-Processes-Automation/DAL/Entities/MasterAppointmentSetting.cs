using System.ComponentModel.DataAnnotations;

namespace Business_Processes_Automation.DAL.Entities;

public class MasterAppointmentSetting : BaseEntity
{
    public int MasterId { get; set; }

    [Range(0, 10080)]
    public int MinBookingNoticeMinutes { get; set; }

    [Range(1, 365)]
    public int MaxBookingDaysAhead { get; set; } = 30;

    [Range(0, 168)]
    public int CancellationPolicyHours { get; set; } = 24;

    [Range(0, 480)]
    public int BufferBetweenClientsMinutes { get; set; }

    [Range(5, 120)]
    public int FreeSlotIntervalMinutes { get; set; } = 15;

    [Range(0, 10)]
    public int MaxRescheduleCount { get; set; } = 2;

    public Master Master { get; set; } = null!;
}
