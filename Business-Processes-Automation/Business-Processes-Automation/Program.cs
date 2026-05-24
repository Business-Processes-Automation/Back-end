
using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.DAL;
using Business_Processes_Automation.DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Business_Processes_Automation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
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
            builder.Services.AddScoped<INotificationChannelRepository, NotificationChannelRepository>();
            builder.Services.AddScoped<INotificationTypeRepository, NotificationTypeRepository>();
            builder.Services.AddScoped<IMasterNotificationPreferenceRepository, MasterNotificationPreferenceRepository>();
            builder.Services.AddScoped<IClientNotificationPreferenceRepository, ClientNotificationPreferenceRepository>();
            builder.Services.AddScoped<IPlatformRepository, PlatformRepository>();
            builder.Services.AddScoped<ISocialAccountRepository, SocialAccountRepository>();
            builder.Services.AddScoped<IAIContentGenerationRepository, AIContentGenerationRepository>();
            builder.Services.AddScoped<IPostPublicationRepository, PostPublicationRepository>();
            builder.Services.AddScoped<IExpenseCategoryRepository, ExpenseCategoryRepository>();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
