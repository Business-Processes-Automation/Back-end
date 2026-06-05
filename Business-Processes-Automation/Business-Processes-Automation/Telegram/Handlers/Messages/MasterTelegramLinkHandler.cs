using Business_Processes_Automation.BLL.Services;
using Business_Processes_Automation.BLL.SessionDrafts;
using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;
using Business_Processes_Automation.Telegram.Configuration;
using Business_Processes_Automation.Telegram.Keyboards;
using Business_Processes_Automation.Telegram.Localization;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Business_Processes_Automation.Telegram.Handlers.Messages;

public class MasterTelegramLinkHandler
{
    private static readonly Regex BotStartParameterRegex = new("^[a-z0-9_]{3,32}$", RegexOptions.Compiled);

    private readonly IMasterTelegramLinkService _linkService;
    private readonly ITelegramUserSessionService _sessionService;
    private readonly IMasterService _masterService;
    private readonly TelegramBotOptions _botOptions;

    public MasterTelegramLinkHandler(
        IMasterTelegramLinkService linkService,
        ITelegramUserSessionService sessionService,
        IMasterService masterService,
        IOptions<TelegramBotOptions> botOptions)
    {
        _linkService = linkService;
        _sessionService = sessionService;
        _masterService = masterService;
        _botOptions = botOptions.Value;
    }

    public async Task<bool> TryHandleAsync(
        ITelegramBotClient botClient,
        Message message,
        long telegramUserId,
        string text,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionService.GetAsync(telegramUserId, cancellationToken);

        if (session is not null && IsLinkingStep(session.CurrentStep))
        {
            await HandleLinkingStepAsync(
                botClient,
                message,
                telegramUserId,
                session,
                text,
                cancellationToken);
            return true;
        }

        if (MasterTelegramLinkKeyboardBuilder.IsCancel(text) && session is not null &&
            IsLinkingStep(session.CurrentStep))
        {
            await CancelLinkingAsync(botClient, message.Chat.Id, telegramUserId, cancellationToken);
            return true;
        }

        return false;
    }

    public async Task StartLinkingAsync(
        ITelegramBotClient botClient,
        Message message,
        CancellationToken cancellationToken = default)
    {
        if (message.From?.Id is not { } telegramUserId)
        {
            return;
        }

        var existing = await _linkService.GetByTelegramUserIdAsync(telegramUserId, cancellationToken);
        if (existing is not null)
        {
            var master = existing.Master
                ?? await _masterService.GetByIdAsync(existing.MasterId, cancellationToken);

            if (master is not null)
            {
                await CompleteAsLinkedMasterAsync(
                    botClient,
                    message.Chat.Id,
                    telegramUserId,
                    master,
                    cancellationToken);
                return;
            }
        }

        var draft = new MasterTelegramLinkDraft
        {
            TelegramUsername = message.From.Username ?? message.From.FirstName ?? "master"
        };

        await _sessionService.SaveMasterTelegramLinkDraftAsync(
            telegramUserId,
            message.Chat.Id,
            draft,
            ConversationStep.MasterLinkingEnterCode,
            cancellationToken);

        await botClient.SendMessage(
            message.Chat.Id,
            TelegramBotTexts.MasterTelegramLink.Intro,
            replyMarkup: MasterTelegramLinkKeyboardBuilder.BuildCancel(),
            cancellationToken: cancellationToken);
    }

    private async Task HandleLinkingStepAsync(
        ITelegramBotClient botClient,
        Message message,
        long telegramUserId,
        TelegramUserSession session,
        string text,
        CancellationToken cancellationToken)
    {
        if (MasterTelegramLinkKeyboardBuilder.IsCancel(text))
        {
            await CancelLinkingAsync(botClient, message.Chat.Id, telegramUserId, cancellationToken);
            return;
        }

        var draft = _sessionService.GetMasterTelegramLinkDraft(session) ?? new MasterTelegramLinkDraft();

        switch (session.CurrentStep)
        {
            case ConversationStep.MasterLinkingEnterCode:
            {
                var validationError = await _linkService.ValidateLinkCodeAsync(text, cancellationToken);
                if (validationError is not null)
                {
                    await botClient.SendMessage(
                        message.Chat.Id,
                        validationError,
                        replyMarkup: MasterTelegramLinkKeyboardBuilder.BuildCancel(),
                        cancellationToken: cancellationToken);
                    return;
                }

                draft.LinkCode = text.Trim().ToUpperInvariant();
                await _sessionService.SaveMasterTelegramLinkDraftAsync(
                    telegramUserId,
                    message.Chat.Id,
                    draft,
                    ConversationStep.MasterLinkingEnterBotSlug,
                    cancellationToken);

                await botClient.SendMessage(
                    message.Chat.Id,
                    TelegramBotTexts.MasterTelegramLink.PromptBotLink,
                    replyMarkup: MasterTelegramLinkKeyboardBuilder.BuildCancel(),
                    cancellationToken: cancellationToken);
                return;
            }

            case ConversationStep.MasterLinkingEnterBotSlug:
            {
                var slug = text.Trim().ToLowerInvariant();
                if (!BotStartParameterRegex.IsMatch(slug))
                {
                    await botClient.SendMessage(
                        message.Chat.Id,
                        TelegramBotTexts.MasterTelegramLink.InvalidBotLink,
                        replyMarkup: MasterTelegramLinkKeyboardBuilder.BuildCancel(),
                        cancellationToken: cancellationToken);
                    return;
                }

                if (await _linkService.IsBotStartParameterTakenAsync(slug, cancellationToken))
                {
                    await botClient.SendMessage(
                        message.Chat.Id,
                        TelegramBotTexts.MasterTelegramLink.BotLinkTaken,
                        replyMarkup: MasterTelegramLinkKeyboardBuilder.BuildCancel(),
                        cancellationToken: cancellationToken);
                    return;
                }

                if (string.IsNullOrWhiteSpace(draft.LinkCode))
                {
                    await botClient.SendMessage(
                        message.Chat.Id,
                        TelegramBotTexts.MasterTelegramLink.SessionExpired,
                        cancellationToken: cancellationToken);
                    await _sessionService.ReturnToMainMenuAsync(telegramUserId, message.Chat.Id, cancellationToken);
                    return;
                }

                var result = await _linkService.LinkTelegramAsync(
                    draft.LinkCode,
                    telegramUserId,
                    draft.TelegramUsername ?? slug,
                    slug,
                    cancellationToken);

                if (!result.Success || result.Master is null || result.MasterTelegram is null)
                {
                    await botClient.SendMessage(
                        message.Chat.Id,
                        result.ErrorMessage ?? TelegramBotTexts.Common.UnexpectedError,
                        replyMarkup: MasterTelegramLinkKeyboardBuilder.BuildCancel(),
                        cancellationToken: cancellationToken);
                    return;
                }

                await _sessionService.BindMasterAsync(
                    telegramUserId,
                    message.Chat.Id,
                    result.Master.Id,
                    TelegramUserRole.Master,
                    cancellationToken);

                var displayName = _masterService.GetDisplayName(result.Master);
                var link = BuildClientLink(result.MasterTelegram.BotStartParameter);

                await botClient.SendMessage(
                    message.Chat.Id,
                    TelegramBotTexts.MasterTelegramLink.Success(displayName, link),
                    replyMarkup: MenuKeyboardBuilder.Build(TelegramUserRole.Master),
                    cancellationToken: cancellationToken);
                return;
            }
        }
    }

    private async Task CancelLinkingAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        CancellationToken cancellationToken)
    {
        await _sessionService.ReturnToMainMenuAsync(telegramUserId, chatId, cancellationToken);

        await botClient.SendMessage(
            chatId,
            TelegramBotTexts.MasterTelegramLink.Cancelled,
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken);
    }

    private async Task CompleteAsLinkedMasterAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        Master master,
        CancellationToken cancellationToken)
    {
        await _sessionService.BindMasterAsync(
            telegramUserId,
            chatId,
            master.Id,
            TelegramUserRole.Master,
            cancellationToken);

        var displayName = _masterService.GetDisplayName(master);
        await botClient.SendMessage(
            chatId,
            TelegramBotTexts.MasterTelegramLink.AlreadyLinked + "\n\n" +
            TelegramBotTexts.Start.WelcomeRegisteredMaster(displayName),
            replyMarkup: MenuKeyboardBuilder.Build(TelegramUserRole.Master),
            cancellationToken: cancellationToken);
    }

    private string BuildClientLink(string botStartParameter)
    {
        var botUsername = _botOptions.BotUsername;
        if (string.IsNullOrWhiteSpace(botUsername))
        {
            return $"?start={botStartParameter}";
        }

        return $"https://t.me/{botUsername.TrimStart('@')}?start={botStartParameter}";
    }

    public static bool IsLinkingStep(ConversationStep step) =>
        step is ConversationStep.MasterLinkingEnterCode or ConversationStep.MasterLinkingEnterBotSlug;
}
