using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.BLL.Interfaces.Repositories;

public interface ITelegramUserSessionRepository
{
    Task<TelegramUserSession?> GetByTelegramUserIdAsync(long telegramUserId, CancellationToken cancellationToken = default);

    Task<TelegramUserSession> UpsertAsync(TelegramUserSession session, CancellationToken cancellationToken = default);
}
