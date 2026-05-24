using System.ComponentModel.DataAnnotations;

namespace Business_Processes_Automation.DAL.Entities;

public class NotificationType : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string TypeName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? TypeDescription { get; set; }

    public ICollection<MasterNotificationPreference> MasterNotificationPreferences { get; set; } = new List<MasterNotificationPreference>();
    public ICollection<ClientNotificationPreference> ClientNotificationPreferences { get; set; } = new List<ClientNotificationPreference>();
}
