using System.Text.Json;
using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.BLL.SessionDrafts;
using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.Services;

public class TelegramUserSessionService : ITelegramUserSessionService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly ITelegramUserSessionRepository _sessionRepository;

    public TelegramUserSessionService(ITelegramUserSessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public Task<TelegramUserSession?> GetAsync(long telegramUserId, CancellationToken cancellationToken = default) =>
        _sessionRepository.GetByTelegramUserIdAsync(telegramUserId, cancellationToken);

    public MasterTelegramLinkDraft? GetMasterTelegramLinkDraft(TelegramUserSession session) =>
        DeserializeDraft<MasterTelegramLinkDraft>(session);

    public ScheduleViewDraft? GetScheduleViewDraft(TelegramUserSession session) =>
        DeserializeDraft<ScheduleViewDraft>(session);

    public BookingDraft? GetBookingDraft(TelegramUserSession session) =>
        DeserializeDraft<BookingDraft>(session);

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
            CurrentStep = ConversationStep.Idle,
            DraftJson = null
        };

        return _sessionRepository.UpsertAsync(session, cancellationToken);
    }

    public Task<TelegramUserSession> SaveMasterTelegramLinkDraftAsync(
        long telegramUserId,
        long chatId,
        MasterTelegramLinkDraft draft,
        ConversationStep step,
        CancellationToken cancellationToken = default) =>
        SaveDraftAsync(telegramUserId, chatId, draft, step, cancellationToken);

    public Task<TelegramUserSession> SaveScheduleViewDraftAsync(
        long telegramUserId,
        long chatId,
        ScheduleViewDraft draft,
        ConversationStep step,
        CancellationToken cancellationToken = default) =>
        SaveDraftAsync(telegramUserId, chatId, draft, step, cancellationToken);

    public Task<TelegramUserSession> SaveBookingDraftAsync(
        long telegramUserId,
        long chatId,
        BookingDraft draft,
        ConversationStep step,
        CancellationToken cancellationToken = default) =>
        SaveDraftAsync(telegramUserId, chatId, draft, step, cancellationToken);

    public Task<TelegramUserSession> SetStepAsync(
        long telegramUserId,
        long chatId,
        ConversationStep step,
        CancellationToken cancellationToken = default) =>
        UpdateSessionAsync(telegramUserId, chatId, step, draftJson: null, clearDraft: false, cancellationToken);

    public async Task<TelegramUserSession> ReturnToMainMenuAsync(
        long telegramUserId,
        long chatId,
        CancellationToken cancellationToken = default)
    {
        var existing = await _sessionRepository.GetByTelegramUserIdAsync(telegramUserId, cancellationToken);

        var session = existing ?? new TelegramUserSession
        {
            TelegramUserId = telegramUserId,
            ChatId = chatId
        };

        session.ChatId = chatId;
        session.CurrentStep = ConversationStep.Idle;
        session.DraftJson = null;

        return await _sessionRepository.UpsertAsync(session, cancellationToken);
    }

    public async Task<TelegramUserSession> ClearSessionAsync(
        long telegramUserId,
        long chatId,
        CancellationToken cancellationToken = default)
    {
        var existing = await _sessionRepository.GetByTelegramUserIdAsync(telegramUserId, cancellationToken);

        if (existing is null)
        {
            return await _sessionRepository.UpsertAsync(
                new TelegramUserSession
                {
                    TelegramUserId = telegramUserId,
                    ChatId = chatId,
                    MasterId = null,
                    Role = TelegramUserRole.Client,
                    CurrentStep = ConversationStep.Idle,
                    DraftJson = null
                },
                cancellationToken);
        }

        existing.ChatId = chatId;
        existing.MasterId = null;
        existing.Role = TelegramUserRole.Client;
        existing.CurrentStep = ConversationStep.Idle;
        existing.DraftJson = null;

        return await _sessionRepository.UpsertAsync(existing, cancellationToken);
    }

    public Task<TelegramUserSession> ResetStepAsync(
        long telegramUserId,
        long chatId,
        CancellationToken cancellationToken = default) =>
        ReturnToMainMenuAsync(telegramUserId, chatId, cancellationToken);

    private static TDraft? DeserializeDraft<TDraft>(TelegramUserSession session)
        where TDraft : class
    {
        if (string.IsNullOrWhiteSpace(session.DraftJson))
        {
            return null;
        }

        return JsonSerializer.Deserialize<TDraft>(session.DraftJson, JsonOptions);
    }

    private Task<TelegramUserSession> SaveDraftAsync<TDraft>(
        long telegramUserId,
        long chatId,
        TDraft draft,
        ConversationStep step,
        CancellationToken cancellationToken) =>
        UpdateSessionAsync(
            telegramUserId,
            chatId,
            step,
            JsonSerializer.Serialize(draft, JsonOptions),
            clearDraft: false,
            cancellationToken);

    private async Task<TelegramUserSession> UpdateSessionAsync(
        long telegramUserId,
        long chatId,
        ConversationStep step,
        string? draftJson,
        bool clearDraft,
        CancellationToken cancellationToken)
    {
        var existing = await _sessionRepository.GetByTelegramUserIdAsync(telegramUserId, cancellationToken);

        var session = existing ?? new TelegramUserSession
        {
            TelegramUserId = telegramUserId,
            ChatId = chatId
        };

        session.ChatId = chatId;
        session.CurrentStep = step;

        if (draftJson is not null)
        {
            session.DraftJson = draftJson;
        }
        else if (clearDraft)
        {
            session.DraftJson = null;
        }

        return await _sessionRepository.UpsertAsync(session, cancellationToken);
    }
}
