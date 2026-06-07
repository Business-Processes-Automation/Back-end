using Business_Processes_Automation.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Business_Processes_Automation.DAL;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Master> Masters => Set<Master>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<MasterAppointmentSetting> MasterAppointmentSettings => Set<MasterAppointmentSetting>();
    public DbSet<WorkingHoursPerDay> WorkingHoursPerDays => Set<WorkingHoursPerDay>();
    public DbSet<TimeOff> TimeOffs => Set<TimeOff>();
    public DbSet<MasterTelegram> MasterTelegrams => Set<MasterTelegram>();
    public DbSet<NotificationChannel> NotificationChannels => Set<NotificationChannel>();
    public DbSet<NotificationType> NotificationTypes => Set<NotificationType>();
    public DbSet<MasterNotificationPreference> MasterNotificationPreferences => Set<MasterNotificationPreference>();
    public DbSet<ClientNotificationPreference> ClientNotificationPreferences => Set<ClientNotificationPreference>();
    public DbSet<ScheduledNotification> ScheduledNotifications => Set<ScheduledNotification>();
    public DbSet<Platform> Platforms => Set<Platform>();
    public DbSet<SocialAccount> SocialAccounts => Set<SocialAccount>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<AIContentGeneration> AiContentGenerations => Set<AIContentGeneration>();
    public DbSet<PostPublication> PostPublications => Set<PostPublication>();
    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<TelegramUserSession> TelegramUserSessions => Set<TelegramUserSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
