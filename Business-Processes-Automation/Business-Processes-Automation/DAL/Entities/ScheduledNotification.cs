using System.ComponentModel.DataAnnotations;
using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.DAL.Entities;

public class ScheduledNotification : BaseEntity
{
    public int AppointmentId { get; set; }

    public ScheduledNotificationKind Kind { get; set; }

    public long TelegramChatId { get; set; }

    [MaxLength(4096)]
    public string MessageText { get; set; } = null!;

    public DateTime ScheduledAtUtc { get; set; }

    public DateTime? SentAtUtc { get; set; }

    public ScheduledNotificationStatus Status { get; set; } = ScheduledNotificationStatus.Pending;

    [MaxLength(2000)]
    public string? LastError { get; set; }

    public Appointment Appointment { get; set; } = null!;
}
