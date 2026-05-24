using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Business_Processes_Automation.DAL.Repositories;

public class TelegramUserSessionRepository : ITelegramUserSessionRepository
{
    private readonly AppDbContext _dbContext;

    public TelegramUserSessionRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<TelegramUserSession?> GetByTelegramUserIdAsync(
        long telegramUserId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.TelegramUserSessions
            .Include(x => x.Master)
            .FirstOrDefaultAsync(x => x.TelegramUserId == telegramUserId, cancellationToken);
    }

    public async Task<TelegramUserSession> UpsertAsync(
        TelegramUserSession session,
        CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.TelegramUserSessions
            .FirstOrDefaultAsync(x => x.TelegramUserId == session.TelegramUserId, cancellationToken);

        if (existing is null)
        {
            session.CreatedAt = DateTime.UtcNow;
            _dbContext.TelegramUserSessions.Add(session);
        }
        else
        {
            existing.ChatId = session.ChatId;
            existing.MasterId = session.MasterId;
            existing.Role = session.Role;
            existing.CurrentStep = session.CurrentStep;
            existing.UpdatedAt = DateTime.UtcNow;
            session = existing;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await _dbContext.TelegramUserSessions
            .Include(x => x.Master)
            .FirstAsync(x => x.Id == session.Id, cancellationToken);
    }
}
