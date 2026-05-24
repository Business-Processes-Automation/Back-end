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

    /// <summary>
    /// Maximum appointments per calendar day. Null means no limit.
    /// </summary>
    [Range(1, 100)]
    public int? MaxAppointmentsPerDay { get; set; }

    /// <summary>
    /// Extra minutes between consecutive client appointments (in addition to service preparation time).
    /// </summary>
    [Range(0, 480)]
    public int BufferBetweenClientsMinutes { get; set; }

    /// <summary>
    /// How many times a client may reschedule a single appointment.
    /// </summary>
    [Range(0, 10)]
    public int MaxRescheduleCount { get; set; } = 1;

    public Master Master { get; set; } = null!;
}
