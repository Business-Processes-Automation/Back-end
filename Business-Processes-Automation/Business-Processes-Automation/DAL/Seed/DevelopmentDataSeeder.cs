using Business_Processes_Automation.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Business_Processes_Automation.DAL.Seed;

public static class DevelopmentDataSeeder
{
    public const string DemoBotStartParameter = "anna_nails";

    public static async Task SeedAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (await dbContext.MasterTelegrams
                .AnyAsync(x => x.BotStartParameter == DemoBotStartParameter, cancellationToken))
        {
            return;
        }

        var master = new Master
        {
            FirstName = "Anna",
            LastName = "Nails",
            Username = "anna_nails",
            Email = "anna.nails@example.com",
            PhoneNumber = "+380501234567",
            TimeZone = "Europe/Kyiv",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Masters.Add(master);
        await dbContext.SaveChangesAsync(cancellationToken);

        dbContext.MasterAppointmentSettings.Add(new MasterAppointmentSetting
        {
            MasterId = master.Id,
            MinBookingNoticeMinutes = 60,
            MaxBookingDaysAhead = 30,
            CancellationPolicyHours = 24,
            BufferBetweenClientsMinutes = 10,
            MaxRescheduleCount = 1,
            CreatedAt = DateTime.UtcNow
        });

        dbContext.MasterTelegrams.Add(new MasterTelegram
        {
            MasterId = master.Id,
            BotStartParameter = DemoBotStartParameter,
            TelegramUserId = 0,
            TelegramUsername = "anna_nails_demo",
            CreatedAt = DateTime.UtcNow
        });

        dbContext.Services.AddRange(
            new Service
            {
                MasterId = master.Id,
                ServiceName = "Манікюр",
                DurationInMinutes = 60,
                Price = 500,
                Prepayment = 100,
                CreatedAt = DateTime.UtcNow
            },
            new Service
            {
                MasterId = master.Id,
                ServiceName = "Педикюр",
                DurationInMinutes = 90,
                Price = 700,
                Prepayment = 150,
                CreatedAt = DateTime.UtcNow
            });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
