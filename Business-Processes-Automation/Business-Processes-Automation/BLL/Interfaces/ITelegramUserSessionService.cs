using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.Services;

public interface ITelegramUserSessionService
{
    Task<TelegramUserSession?> GetAsync(long telegramUserId, CancellationToken cancellationToken = default);

    Task<TelegramUserSession> BindMasterAsync(
        long telegramUserId,
        long chatId,
        int masterId,
        TelegramUserRole role,
        CancellationToken cancellationToken = default);

    Task<TelegramUserSession> ResetStepAsync(
        long telegramUserId,
        long chatId,
        CancellationToken cancellationToken = default);
}
