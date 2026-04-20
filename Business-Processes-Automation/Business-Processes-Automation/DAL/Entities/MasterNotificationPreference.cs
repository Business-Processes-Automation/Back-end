namespace Business_Processes_Automation.DAL.Entities;

public class MasterNotificationPreference : BaseEntity
{
    public int MasterId { get; set; }
    public int NotificationChannelId { get; set; }
    public int NotificationTypeId { get; set; }
    public bool IsEnabled { get; set; } = true;

    // Navigation property
    public Master Master { get; set; } = null!;
    public NotificationChannel NotificationChannel { get; set; } = null!;
    public NotificationType NotificationType { get; set; } = null!;
}
