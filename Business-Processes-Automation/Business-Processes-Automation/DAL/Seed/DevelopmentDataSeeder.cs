using Business_Processes_Automation.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Business_Processes_Automation.DAL.Seed;

public static class DevelopmentDataSeeder
{
    public const string AnnaBotStartParameter = "anna_nails";
    public const string ViolettaBotStartParameter = "violetta";

    private const long ViolettaTelegramUserId = 648810193;

    public static async Task SeedAsync(
        AppDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        await SeedAnnaAsync(dbContext, cancellationToken);
        await SeedViolettaAsync(dbContext, cancellationToken);
    }

    private static async Task SeedAnnaAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        if (await dbContext.MasterTelegrams
                .AnyAsync(x => x.BotStartParameter == AnnaBotStartParameter, cancellationToken))
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

        AddDefaultAppointmentSettings(dbContext, master.Id);

        dbContext.MasterTelegrams.Add(new MasterTelegram
        {
            MasterId = master.Id,
            BotStartParameter = AnnaBotStartParameter,
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

    private static async Task SeedViolettaAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        if (await dbContext.MasterTelegrams
                .AnyAsync(x => x.BotStartParameter == ViolettaBotStartParameter, cancellationToken))
        {
            return;
        }

        if (await dbContext.MasterTelegrams
                .AnyAsync(x => x.TelegramUserId == ViolettaTelegramUserId, cancellationToken))
        {
            return;
        }

        var master = new Master
        {
            FirstName = "Violetta",
            LastName = "Master",
            Username = "violetta",
            Email = "violetta@example.com",
            PhoneNumber = "+380509999999",
            TimeZone = "Europe/Kyiv",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Masters.Add(master);
        await dbContext.SaveChangesAsync(cancellationToken);

        AddDefaultAppointmentSettings(dbContext, master.Id);

        dbContext.MasterTelegrams.Add(new MasterTelegram
        {
            MasterId = master.Id,
            BotStartParameter = ViolettaBotStartParameter,
            TelegramUserId = ViolettaTelegramUserId,
            TelegramUsername = "violetta",
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static void AddDefaultAppointmentSettings(AppDbContext dbContext, int masterId)
    {
        dbContext.MasterAppointmentSettings.Add(new MasterAppointmentSetting
        {
            MasterId = masterId,
            MinBookingNoticeMinutes = 60,
            MaxBookingDaysAhead = 30,
            CancellationPolicyHours = 24,
            BufferBetweenClientsMinutes = 10,
            MaxRescheduleCount = 1,
            CreatedAt = DateTime.UtcNow
        });
    }
}
