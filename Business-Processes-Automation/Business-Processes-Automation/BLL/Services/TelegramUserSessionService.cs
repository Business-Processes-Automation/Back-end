using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.Services;

public class TelegramUserSessionService(ITelegramUserSessionRepository sessionRepository) : ITelegramUserSessionService
{
    public Task<TelegramUserSession?> GetAsync(long telegramUserId, CancellationToken cancellationToken = default) =>
        sessionRepository.GetByTelegramUserIdAsync(telegramUserId, cancellationToken);

    public Task<TelegramUserSession> BindMasterAsync(
        long telegramUserId,
        long chatId,
        int masterId,
        TelegramUserRole role,
        CancellationToken cancellationToken = default)
    {
        var session = new TelegramUserSession
        {
            TelegramUserId = telegramUserId,
            ChatId = chatId,
            MasterId = masterId,
            Role = role,
            CurrentStep = ConversationStep.Idle
        };

        return sessionRepository.UpsertAsync(session, cancellationToken);
    }

    public async Task<TelegramUserSession> ResetStepAsync(
        long telegramUserId,
        long chatId,
        CancellationToken cancellationToken = default)
    {
        var existing = await sessionRepository.GetByTelegramUserIdAsync(telegramUserId, cancellationToken);

        var session = existing ?? new TelegramUserSession
        {
            TelegramUserId = telegramUserId,
            ChatId = chatId
        };

        session.ChatId = chatId;
        session.CurrentStep = ConversationStep.Idle;

        return await sessionRepository.UpsertAsync(session, cancellationToken);
    }
}
