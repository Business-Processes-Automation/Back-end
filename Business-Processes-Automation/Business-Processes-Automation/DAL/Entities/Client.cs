using System.ComponentModel.DataAnnotations;

namespace Business_Processes_Automation.DAL.Entities;

public class Client : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string ClientName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    [Phone]
    public string ClientPhone { get; set; } = string.Empty;

    public long? ClientTelegramId { get; set; }

    [MaxLength(256)]
    [EmailAddress]
    public string? ClientEmail { get; set; }

    /// <summary>
    /// Available deposit from cancelled prepayments (SRS cancellation policy).
    /// </summary>
    [Range(typeof(decimal), "0", "999999.99")]
    public decimal DepositBalance { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<ClientNotificationPreference> NotificationPreferences { get; set; } = new List<ClientNotificationPreference>();
}
