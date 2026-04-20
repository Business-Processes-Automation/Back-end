namespace Business_Processes_Automation.DAL.Entities;

public class Client : BaseEntity
{
    public string ClientName { get; set; } = string.Empty;
    public string ClientPhone { get; set; } = string.Empty;
    public string? ClientTelegram { get; set; }
    public string? ClientEmail { get; set; }

    // Navigation property
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<ClientNotificationPreference> NotificationPreferences { get; set; } = new List<ClientNotificationPreference>();
}
