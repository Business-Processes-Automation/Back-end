using Business_Processes_Automation.BLL.SessionDrafts;
using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.Services;

public interface ITelegramUserSessionService
{
    Task<TelegramUserSession?> GetAsync(long telegramUserId, CancellationToken cancellationToken = default);

    MasterServiceDraft? GetMasterServiceDraft(TelegramUserSession session);

    MasterRegistrationDraft? GetMasterRegistrationDraft(TelegramUserSession session);

    Task<TelegramUserSession> BindMasterAsync(
        long telegramUserId,
        long chatId,
        int masterId,
        TelegramUserRole role,
        CancellationToken cancellationToken = default);

    Task<TelegramUserSession> SaveMasterServiceDraftAsync(
        long telegramUserId,
        long chatId,
        MasterServiceDraft draft,
        ConversationStep step,
        CancellationToken cancellationToken = default);

    Task<TelegramUserSession> SaveMasterRegistrationDraftAsync(
        long telegramUserId,
        long chatId,
        MasterRegistrationDraft draft,
        ConversationStep step,
        CancellationToken cancellationToken = default);

    Task<TelegramUserSession> SetStepAsync(
        long telegramUserId,
        long chatId,
        ConversationStep step,
        CancellationToken cancellationToken = default);

    Task<TelegramUserSession> ReturnToMainMenuAsync(
        long telegramUserId,
        long chatId,
        CancellationToken cancellationToken = default);

    Task<TelegramUserSession> ClearSessionAsync(
        long telegramUserId,
        long chatId,
        CancellationToken cancellationToken = default);

    Task<TelegramUserSession> ResetStepAsync(
        long telegramUserId,
        long chatId,
        CancellationToken cancellationToken = default);
}
