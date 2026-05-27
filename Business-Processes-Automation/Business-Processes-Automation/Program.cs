
using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.BLL.Services;
using Business_Processes_Automation.DAL;
using Business_Processes_Automation.DAL.Repositories;
using Business_Processes_Automation.Telegram.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Business_Processes_Automation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddJsonFile(
                "appsettings.Development.local.json",
                optional: true,
                reloadOnChange: true);

            builder.Services.AddTelegramBot(builder.Configuration);

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sql => sql.EnableRetryOnFailure(maxRetryCount: 3)));

            builder.Services.AddScoped<IMasterRepository, MasterRepository>();
            builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
            builder.Services.AddScoped<IClientRepository, ClientRepository>();
            builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
            builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
            builder.Services.AddScoped<IPostRepository, PostRepository>();
            builder.Services.AddScoped<IMasterAppointmentSettingRepository, MasterAppointmentSettingRepository>();
            builder.Services.AddScoped<IWorkingHoursPerDayRepository, WorkingHoursPerDayRepository>();
            builder.Services.AddScoped<ITimeOffRepository, TimeOffRepository>();
            builder.Services.AddScoped<IMasterTelegramRepository, MasterTelegramRepository>();
            builder.Services.AddScoped<ITelegramUserSessionRepository, TelegramUserSessionRepository>();
            builder.Services.AddScoped<IMasterService, MasterService>();
            builder.Services.AddScoped<IServiceManagementService, ServiceManagementService>();
            builder.Services.AddScoped<IMasterRegistrationService, MasterRegistrationService>();
            builder.Services.AddScoped<IMasterAccountService, MasterAccountService>();
            builder.Services.AddScoped<ITelegramUserSessionService, TelegramUserSessionService>();
            builder.Services.AddScoped<IMasterScheduleSettingsService, MasterScheduleSettingsService>();
            builder.Services.AddScoped<IMasterAvailabilityService, MasterAvailabilityService>();
            builder.Services.AddScoped<IMasterScheduleViewService, MasterScheduleViewService>();
            builder.Services.AddScoped<IClientBookingService, ClientBookingService>();
            builder.Services.AddScoped<IClientAppointmentsService, ClientAppointmentsService>();
            builder.Services.AddScoped<INotificationChannelRepository, NotificationChannelRepository>();
            builder.Services.AddScoped<INotificationTypeRepository, NotificationTypeRepository>();
            builder.Services.AddScoped<IMasterNotificationPreferenceRepository, MasterNotificationPreferenceRepository>();
            builder.Services.AddScoped<IClientNotificationPreferenceRepository, ClientNotificationPreferenceRepository>();
            builder.Services.AddScoped<IPlatformRepository, PlatformRepository>();
            builder.Services.AddScoped<ISocialAccountRepository, SocialAccountRepository>();
            builder.Services.AddScoped<IAIContentGenerationRepository, AIContentGenerationRepository>();
            builder.Services.AddScoped<IPostPublicationRepository, PostPublicationRepository>();
            builder.Services.AddScoped<IExpenseCategoryRepository, ExpenseCategoryRepository>();

            var app = builder.Build();

            app.Run();
        }
    }
}
