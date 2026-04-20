namespace Business_Processes_Automation.DAL.Entities;

public class NotificationType : BaseEntity
{
    public string TypeName { get; set; } = string.Empty;
    public string? TypeDescription { get; set; }

    // Navigation property
    public ICollection<MasterNotificationPreference> MasterNotificationPreferences { get; set; } = new List<MasterNotificationPreference>();
    public ICollection<ClientNotificationPreference> ClientNotificationPreferences { get; set; } = new List<ClientNotificationPreference>();
}
