namespace Business_Processes_Automation.DAL.Entities;

public class NotificationChannel : BaseEntity
{
    public string NotificationName { get; set; } = string.Empty;
    public string? NotificationDescription { get; set; }

    // Navigation property
    public ICollection<MasterNotificationPreference> MasterNotificationPreferences { get; set; } = new List<MasterNotificationPreference>();
    public ICollection<ClientNotificationPreference> ClientNotificationPreferences { get; set; } = new List<ClientNotificationPreference>();
}
