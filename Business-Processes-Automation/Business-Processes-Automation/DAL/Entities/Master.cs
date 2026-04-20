namespace Business_Processes_Automation.DAL.Entities;

public class Master : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string TimeZone { get; set; } = "UTC";
    public bool IsActive { get; set; } = true;

    // Navigation property
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
