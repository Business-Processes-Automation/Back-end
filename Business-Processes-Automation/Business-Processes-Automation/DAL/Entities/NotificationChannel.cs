using System.ComponentModel.DataAnnotations;

namespace Business_Processes_Automation.DAL.Entities;

public class NotificationChannel : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string NotificationName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? NotificationDescription { get; set; }

    public ICollection<MasterNotificationPreference> MasterNotificationPreferences { get; set; } = new List<MasterNotificationPreference>();
    public ICollection<ClientNotificationPreference> ClientNotificationPreferences { get; set; } = new List<ClientNotificationPreference>();
}
