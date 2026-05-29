using System.ComponentModel.DataAnnotations;

namespace Business_Processes_Automation.DAL.Entities;

public class Master : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(512)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string TimeZone { get; set; } = "UTC";

    public bool IsActive { get; set; } = true;

    public MasterAppointmentSetting? AppointmentSetting { get; set; }
    public MasterTelegram? MasterTelegram { get; set; }
    public ICollection<Service> Services { get; set; } = new List<Service>();
    public ICollection<WorkingHoursPerDay> WorkingHours { get; set; } = new List<WorkingHoursPerDay>();
    public ICollection<TimeOff> TimeOffs { get; set; } = new List<TimeOff>();
    public ICollection<MasterNotificationPreference> NotificationPreferences { get; set; } = new List<MasterNotificationPreference>();
    public ICollection<SocialAccount> SocialAccounts { get; set; } = new List<SocialAccount>();
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
