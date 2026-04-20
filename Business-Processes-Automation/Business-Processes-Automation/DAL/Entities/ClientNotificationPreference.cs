namespace Business_Processes_Automation.DAL.Entities;

public class ClientNotificationPreference : BaseEntity
{
    public int ClientId { get; set; }
    public int NotificationChannelId { get; set; }
    public int NotificationTypeId { get; set; }
    public bool IsEnabled { get; set; } = true;

    // Navigation property
    public Client Client { get; set; } = null!;
    public NotificationChannel NotificationChannel { get; set; } = null!;
    public NotificationType NotificationType { get; set; } = null!;
}
