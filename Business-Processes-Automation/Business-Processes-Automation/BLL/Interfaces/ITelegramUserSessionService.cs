using Business_Processes_Automation.BLL.SessionDrafts;
using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.Services;

public interface ITelegramUserSessionService
{
    Task<TelegramUserSession?> GetAsync(long telegramUserId, CancellationToken cancellationToken = default);

    MasterTelegramLinkDraft? GetMasterTelegramLinkDraft(TelegramUserSession session);

    ScheduleViewDraft? GetScheduleViewDraft(TelegramUserSession session);

    BookingDraft? GetBookingDraft(TelegramUserSession session);

    Task<TelegramUserSession> BindMasterAsync(
        long telegramUserId,
        long chatId,
        int masterId,
        TelegramUserRole role,
        CancellationToken cancellationToken = default);

    Task<TelegramUserSession> SaveMasterTelegramLinkDraftAsync(
        long telegramUserId,
        long chatId,
        MasterTelegramLinkDraft draft,
        ConversationStep step,
        CancellationToken cancellationToken = default);

    Task<TelegramUserSession> SaveScheduleViewDraftAsync(
        long telegramUserId,
        long chatId,
        ScheduleViewDraft draft,
        ConversationStep step,
        CancellationToken cancellationToken = default);

    Task<TelegramUserSession> SaveBookingDraftAsync(
        long telegramUserId,
        long chatId,
        BookingDraft draft,
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
