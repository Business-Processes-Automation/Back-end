using Business_Processes_Automation.BLL.Services;
using Business_Processes_Automation.BLL.SessionDrafts;
using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;
using Business_Processes_Automation.Telegram.Configuration;
using Business_Processes_Automation.Telegram.Keyboards;
using Business_Processes_Automation.Telegram.Localization;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Business_Processes_Automation.Telegram.Handlers.Messages;

public class MasterRegistrationHandler
{
    private static readonly Regex BotStartParameterRegex = new("^[a-z0-9_]{3,32}$", RegexOptions.Compiled);

    private readonly IMasterRegistrationService _registrationService;
    private readonly ITelegramUserSessionService _sessionService;
    private readonly IMasterService _masterService;
    private readonly TelegramBotOptions _botOptions;

    public MasterRegistrationHandler(
        IMasterRegistrationService registrationService,
        ITelegramUserSessionService sessionService,
        IMasterService masterService,
        IOptions<TelegramBotOptions> botOptions)
    {
        _registrationService = registrationService;
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

        if (session is not null && IsRegistrationStep(session.CurrentStep))
        {
            await HandleRegistrationStepAsync(
                botClient,
                message,
                telegramUserId,
                session,
                text,
                cancellationToken);
            return true;
        }

        if (MasterRegistrationKeyboardBuilder.IsCancel(text) && session is not null)
        {
            await CancelRegistrationAsync(botClient, message.Chat.Id, telegramUserId, cancellationToken);
            return true;
        }

        return false;
    }

    public async Task StartRegistrationAsync(
        ITelegramBotClient botClient,
        Message message,
        CancellationToken cancellationToken = default)
    {
        if (message.From?.Id is not { } telegramUserId)
        {
            return;
        }

        var existing = await _registrationService.GetByTelegramUserIdAsync(telegramUserId, cancellationToken);
        if (existing?.Master is not null)
        {
            await CompleteAsRegisteredMasterAsync(
                botClient,
                message.Chat.Id,
                telegramUserId,
                existing.Master,
                cancellationToken);
            return;
        }

        var draft = new MasterRegistrationDraft
        {
            TelegramUsername = message.From.Username ?? message.From.FirstName ?? "master"
        };

        await _sessionService.SaveMasterRegistrationDraftAsync(
            telegramUserId,
            message.Chat.Id,
            draft,
            ConversationStep.MasterRegisteringFirstName,
            cancellationToken);

        await botClient.SendMessage(
            message.Chat.Id,
            TelegramBotTexts.MasterRegistration.Intro,
            cancellationToken: cancellationToken);
    }

    private async Task HandleRegistrationStepAsync(
        ITelegramBotClient botClient,
        Message message,
        long telegramUserId,
        DAL.Entities.TelegramUserSession session,
        string text,
        CancellationToken cancellationToken)
    {
        if (MasterRegistrationKeyboardBuilder.IsCancel(text))
        {
            await CancelRegistrationAsync(botClient, message.Chat.Id, telegramUserId, cancellationToken);
            return;
        }

        var draft = _sessionService.GetMasterRegistrationDraft(session) ?? new MasterRegistrationDraft();

        switch (session.CurrentStep)
        {
            case ConversationStep.MasterRegisteringFirstName:
                if (string.IsNullOrWhiteSpace(text) || text.Trim().Length > 100)
                {
                    await botClient.SendMessage(
                        message.Chat.Id,
                        TelegramBotTexts.MasterRegistration.InvalidFirstName,
                        cancellationToken: cancellationToken);
                    return;
                }

                draft.FirstName = text.Trim();
                await SaveAndPromptAsync(
                    botClient,
                    telegramUserId,
                    message.Chat.Id,
                    draft,
                    ConversationStep.MasterRegisteringLastName,
                    TelegramBotTexts.MasterRegistration.PromptLastName,
                    cancellationToken);
                return;

            case ConversationStep.MasterRegisteringLastName:
                if (string.IsNullOrWhiteSpace(text) || text.Trim().Length > 100)
                {
                    await botClient.SendMessage(
                        message.Chat.Id,
                        TelegramBotTexts.MasterRegistration.InvalidLastName,
                        cancellationToken: cancellationToken);
                    return;
                }

                draft.LastName = text.Trim();
                await SaveAndPromptAsync(
                    botClient,
                    telegramUserId,
                    message.Chat.Id,
                    draft,
                    ConversationStep.MasterRegisteringPhone,
                    TelegramBotTexts.MasterRegistration.PromptPhone,
                    cancellationToken);
                return;

            case ConversationStep.MasterRegisteringPhone:
                if (string.IsNullOrWhiteSpace(text) || text.Trim().Length is < 5 or > 20)
                {
                    await botClient.SendMessage(
                        message.Chat.Id,
                        TelegramBotTexts.MasterRegistration.InvalidPhone,
                        cancellationToken: cancellationToken);
                    return;
                }

                draft.PhoneNumber = text.Trim();
                await SaveAndPromptAsync(
                    botClient,
                    telegramUserId,
                    message.Chat.Id,
                    draft,
                    ConversationStep.MasterRegisteringEmail,
                    TelegramBotTexts.MasterRegistration.PromptEmail,
                    cancellationToken);
                return;

            case ConversationStep.MasterRegisteringEmail:
                if (string.IsNullOrWhiteSpace(text) ||
                    !new EmailAddressAttribute().IsValid(text.Trim()))
                {
                    await botClient.SendMessage(
                        message.Chat.Id,
                        TelegramBotTexts.MasterRegistration.InvalidEmail,
                        cancellationToken: cancellationToken);
                    return;
                }

                draft.Email = text.Trim();
                await SaveAndPromptAsync(
                    botClient,
                    telegramUserId,
                    message.Chat.Id,
                    draft,
                    ConversationStep.MasterRegisteringTimeZone,
                    TelegramBotTexts.MasterRegistration.PromptTimeZone,
                    cancellationToken);
                return;

            case ConversationStep.MasterRegisteringTimeZone:
                draft.TimeZone = text.Trim() is "-" or "" ? "Europe/Kyiv" : text.Trim();
                if (draft.TimeZone.Length > 64)
                {
                    await botClient.SendMessage(
                        message.Chat.Id,
                        TelegramBotTexts.MasterRegistration.PromptTimeZone,
                        cancellationToken: cancellationToken);
                    return;
                }

                await SaveAndPromptAsync(
                    botClient,
                    telegramUserId,
                    message.Chat.Id,
                    draft,
                    ConversationStep.MasterRegisteringBotLink,
                    TelegramBotTexts.MasterRegistration.PromptBotLink,
                    cancellationToken);
                return;

            case ConversationStep.MasterRegisteringBotLink:
                var slug = text.Trim().ToLowerInvariant();
                if (!BotStartParameterRegex.IsMatch(slug))
                {
                    await botClient.SendMessage(
                        message.Chat.Id,
                        TelegramBotTexts.MasterRegistration.InvalidBotLink,
                        cancellationToken: cancellationToken);
                    return;
                }

                if (await _registrationService.IsBotStartParameterTakenAsync(slug, cancellationToken))
                {
                    await botClient.SendMessage(
                        message.Chat.Id,
                        TelegramBotTexts.MasterRegistration.BotLinkTaken,
                        cancellationToken: cancellationToken);
                    return;
                }

                draft.BotStartParameter = slug;
                await _sessionService.SaveMasterRegistrationDraftAsync(
                    telegramUserId,
                    message.Chat.Id,
                    draft,
                    ConversationStep.MasterRegisteringConfirm,
                    cancellationToken);

                var summary = BuildSummary(draft);
                await botClient.SendMessage(
                    message.Chat.Id,
                    TelegramBotTexts.MasterRegistration.ConfirmationSummary(summary),
                    replyMarkup: MasterRegistrationKeyboardBuilder.BuildConfirmation(),
                    cancellationToken: cancellationToken);
                return;

            case ConversationStep.MasterRegisteringConfirm:
                if (!MasterRegistrationKeyboardBuilder.IsConfirm(text))
                {
                    await botClient.SendMessage(
                        message.Chat.Id,
                        "Натисніть «Підтвердити реєстрацію» або «Скасувати реєстрацію».",
                        replyMarkup: MasterRegistrationKeyboardBuilder.BuildConfirmation(),
                        cancellationToken: cancellationToken);
                    return;
                }

                var result = await _registrationService.RegisterAsync(telegramUserId, draft, cancellationToken);
                if (!result.Success || result.Master is null || result.MasterTelegram is null)
                {
                    await botClient.SendMessage(
                        message.Chat.Id,
                        result.ErrorMessage ?? TelegramBotTexts.Common.UnexpectedError,
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
                    TelegramBotTexts.MasterRegistration.Success(displayName, link),
                    replyMarkup: MenuKeyboardBuilder.Build(TelegramUserRole.Master),
                    cancellationToken: cancellationToken);
                return;
        }
    }

    private async Task SaveAndPromptAsync(
        ITelegramBotClient botClient,
        long telegramUserId,
        long chatId,
        MasterRegistrationDraft draft,
        ConversationStep step,
        string prompt,
        CancellationToken cancellationToken)
    {
        await _sessionService.SaveMasterRegistrationDraftAsync(
            telegramUserId,
            chatId,
            draft,
            step,
            cancellationToken);

        await botClient.SendMessage(chatId, prompt, cancellationToken: cancellationToken);
    }

    private async Task CancelRegistrationAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        CancellationToken cancellationToken)
    {
        await _sessionService.ReturnToMainMenuAsync(telegramUserId, chatId, cancellationToken);

        await botClient.SendMessage(
            chatId,
            TelegramBotTexts.MasterRegistration.Cancelled,
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken);
    }

    private async Task CompleteAsRegisteredMasterAsync(
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
            TelegramBotTexts.MasterRegistration.AlreadyRegistered + "\n\n" +
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

    private static TelegramBotTexts.MasterRegistrationSummary BuildSummary(MasterRegistrationDraft draft) =>
        new()
        {
            FirstName = draft.FirstName ?? "",
            LastName = draft.LastName ?? "",
            PhoneNumber = draft.PhoneNumber ?? "",
            Email = draft.Email ?? "",
            TimeZone = draft.TimeZone ?? "Europe/Kyiv",
            BotLinkLabel = draft.BotStartParameter ?? ""
        };

    public static bool IsRegistrationStep(ConversationStep step) =>
        step is ConversationStep.MasterRegisteringFirstName
            or ConversationStep.MasterRegisteringLastName
            or ConversationStep.MasterRegisteringPhone
            or ConversationStep.MasterRegisteringEmail
            or ConversationStep.MasterRegisteringTimeZone
            or ConversationStep.MasterRegisteringBotLink
            or ConversationStep.MasterRegisteringConfirm;
}
