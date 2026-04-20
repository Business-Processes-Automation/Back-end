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
    public DbSet<Platform> Platforms => Set<Platform>();
    public DbSet<SocialAccount> SocialAccounts => Set<SocialAccount>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<AIContentGeneration> AiContentGenerations => Set<AIContentGeneration>();
    public DbSet<PostPublication> PostPublications => Set<PostPublication>();
    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
    public DbSet<Expense> Expenses => Set<Expense>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>()
            .Property(x => x.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Payment>()
            .Property(x => x.PaymentMethod)
            .HasConversion<string>();

        modelBuilder.Entity<Payment>()
            .Property(x => x.PaymentStatus)
            .HasConversion<string>();

        modelBuilder.Entity<Payment>()
            .Property(x => x.PaymentType)
            .HasConversion<string>();

        modelBuilder.Entity<Post>()
            .Property(x => x.Status)
            .HasConversion<string>();

        modelBuilder.Entity<PostPublication>()
            .Property(x => x.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Service>()
            .HasOne(x => x.Master)
            .WithMany(x => x.Services)
            .HasForeignKey(x => x.MasterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(x => x.Client)
            .WithMany(x => x.Appointments)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(x => x.Service)
            .WithMany(x => x.Appointments)
            .HasForeignKey(x => x.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Payment>()
            .HasOne(x => x.Appointment)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.AppointmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MasterAppointmentSetting>()
            .HasOne(x => x.Master)
            .WithOne(x => x.AppointmentSetting)
            .HasForeignKey<MasterAppointmentSetting>(x => x.MasterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MasterTelegram>()
            .HasOne(x => x.Master)
            .WithOne(x => x.MasterTelegram)
            .HasForeignKey<MasterTelegram>(x => x.MasterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<WorkingHoursPerDay>()
            .HasOne(x => x.Master)
            .WithMany(x => x.WorkingHours)
            .HasForeignKey(x => x.MasterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TimeOff>()
            .HasOne(x => x.Master)
            .WithMany(x => x.TimeOffs)
            .HasForeignKey(x => x.MasterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MasterNotificationPreference>()
            .HasOne(x => x.Master)
            .WithMany(x => x.NotificationPreferences)
            .HasForeignKey(x => x.MasterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MasterNotificationPreference>()
            .HasOne(x => x.NotificationChannel)
            .WithMany(x => x.MasterNotificationPreferences)
            .HasForeignKey(x => x.NotificationChannelId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MasterNotificationPreference>()
            .HasOne(x => x.NotificationType)
            .WithMany(x => x.MasterNotificationPreferences)
            .HasForeignKey(x => x.NotificationTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ClientNotificationPreference>()
            .HasOne(x => x.Client)
            .WithMany(x => x.NotificationPreferences)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ClientNotificationPreference>()
            .HasOne(x => x.NotificationChannel)
            .WithMany(x => x.ClientNotificationPreferences)
            .HasForeignKey(x => x.NotificationChannelId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ClientNotificationPreference>()
            .HasOne(x => x.NotificationType)
            .WithMany(x => x.ClientNotificationPreferences)
            .HasForeignKey(x => x.NotificationTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SocialAccount>()
            .HasOne(x => x.Master)
            .WithMany(x => x.SocialAccounts)
            .HasForeignKey(x => x.MasterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SocialAccount>()
            .HasOne(x => x.Platform)
            .WithMany(x => x.SocialAccounts)
            .HasForeignKey(x => x.PlatformId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Post>()
            .HasOne(x => x.Master)
            .WithMany(x => x.Posts)
            .HasForeignKey(x => x.MasterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AIContentGeneration>()
            .HasOne(x => x.Post)
            .WithMany(x => x.AiContentGenerations)
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PostPublication>()
            .HasOne(x => x.Post)
            .WithMany(x => x.PostPublications)
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PostPublication>()
            .HasOne(x => x.Platform)
            .WithMany(x => x.PostPublications)
            .HasForeignKey(x => x.PlatformId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Expense>()
            .HasOne(x => x.Master)
            .WithMany(x => x.Expenses)
            .HasForeignKey(x => x.MasterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Expense>()
            .HasOne(x => x.ExpenseCategory)
            .WithMany(x => x.Expenses)
            .HasForeignKey(x => x.ExpenseCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Service>()
            .HasIndex(x => new { x.MasterId, x.ServiceName })
            .IsUnique(false);

        modelBuilder.Entity<Appointment>()
            .HasIndex(x => new { x.ServiceId, x.StartDateTime });

        modelBuilder.Entity<WorkingHoursPerDay>()
            .HasIndex(x => new { x.MasterId, x.DayOfWeek })
            .IsUnique();

        modelBuilder.Entity<SocialAccount>()
            .HasIndex(x => new { x.MasterId, x.PlatformId })
            .IsUnique();

        modelBuilder.Entity<PostPublication>()
            .HasIndex(x => new { x.PostId, x.PlatformId })
            .IsUnique();

        // Global soft-delete filters.
        modelBuilder.Entity<Master>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Service>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Client>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Appointment>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Payment>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<MasterAppointmentSetting>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<WorkingHoursPerDay>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<TimeOff>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<MasterTelegram>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<NotificationChannel>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<NotificationType>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<MasterNotificationPreference>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<ClientNotificationPreference>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Platform>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<SocialAccount>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Post>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<AIContentGeneration>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<PostPublication>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<ExpenseCategory>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Expense>().HasQueryFilter(x => !x.IsDeleted);
    }
}
