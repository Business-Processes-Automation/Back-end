using Business_Processes_Automation.DAL;
using Business_Processes_Automation.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Business_Processes_Automation.BLL.Services;

public class MasterAccountService : IMasterAccountService
{
    private readonly AppDbContext _dbContext;
    private readonly ITelegramUserSessionService _sessionService;

    public MasterAccountService(
        AppDbContext dbContext,
        ITelegramUserSessionService sessionService)
    {
        _dbContext = dbContext;
        _sessionService = sessionService;
    }

    public Task<MasterAccountResult> LogoutAsync(
        long telegramUserId,
        long chatId,
        CancellationToken cancellationToken = default) =>
        ClearSessionAsync(telegramUserId, chatId, cancellationToken);

    public async Task<MasterAccountResult> DeleteAccountAsync(
        long telegramUserId,
        CancellationToken cancellationToken = default)
    {
        var masterTelegram = await _dbContext.MasterTelegrams
            .IgnoreQueryFilters()
            .Include(x => x.Master)
            .FirstOrDefaultAsync(x => x.TelegramUserId == telegramUserId && !x.IsDeleted, cancellationToken);

        if (masterTelegram is null)
        {
            return MasterAccountResult.Fail("Обліковий запис майстра не знайдено.");
        }

        var now = DateTime.UtcNow;

        masterTelegram.IsDeleted = true;
        masterTelegram.UpdatedAt = now;

        if (masterTelegram.Master is { } master)
        {
            master.IsDeleted = true;
            master.UpdatedAt = now;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var session = await _dbContext.TelegramUserSessions
            .FirstOrDefaultAsync(x => x.TelegramUserId == telegramUserId, cancellationToken);

        if (session?.ChatId is { } chatId)
        {
            await _sessionService.ClearSessionAsync(telegramUserId, chatId, cancellationToken);
        }

        return MasterAccountResult.Ok();
    }

    private async Task<MasterAccountResult> ClearSessionAsync(
        long telegramUserId,
        long chatId,
        CancellationToken cancellationToken)
    {
        await _sessionService.ClearSessionAsync(telegramUserId, chatId, cancellationToken);
        return MasterAccountResult.Ok();
    }
}
