using Business_Processes_Automation.BLL.Interfaces;
using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.BLL.Services;
using Business_Processes_Automation.DAL;
using Business_Processes_Automation.DAL.Repositories;
using Business_Processes_Automation.Telegram.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Http;
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

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(
                        new System.Text.Json.Serialization.JsonStringEnumConverter());
                });
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sql => sql.EnableRetryOnFailure(maxRetryCount: 3)));

            var allowedOrigins = builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? ["http://localhost:3000"];

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("Frontend", policy =>
                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());
            });

            builder.Services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                options.Secure = CookieSecurePolicy.Always;
            });

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.Path = "/";
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.LoginPath = "/api/auth/login";
            });

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
            builder.Services.AddScoped<IMasterTelegramLinkService, MasterTelegramLinkService>();
            builder.Services.AddScoped<IMasterAccountService, MasterAccountService>();
            builder.Services.AddScoped<ITelegramUserSessionService, TelegramUserSessionService>();
            builder.Services.AddScoped<IMasterScheduleSettingsService, MasterScheduleSettingsService>();
            builder.Services.AddScoped<IMasterAvailabilityService, MasterAvailabilityService>();
            builder.Services.AddScoped<IMasterScheduleViewService, MasterScheduleViewService>();
            builder.Services.AddScoped<IMasterScheduleApiService, MasterScheduleApiService>();
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
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCookiePolicy();

            app.UseCors("Frontend");

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}