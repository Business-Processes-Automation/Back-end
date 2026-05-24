using Business_Processes_Automation.BLL.SessionDrafts;
using Business_Processes_Automation.BLL.Services;
using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;
using Business_Processes_Automation.Telegram.Keyboards;
using Business_Processes_Automation.Telegram.Localization;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Business_Processes_Automation.Telegram.Handlers.Messages;

public class MasterPanelHandler
{
    private readonly ITelegramUserSessionService _sessionService;
    private readonly IServiceManagementService _serviceManagementService;
    private readonly IMasterAccountService _accountService;

    public MasterPanelHandler(
        ITelegramUserSessionService sessionService,
        IServiceManagementService serviceManagementService,
        IMasterAccountService accountService)
    {
        _sessionService = sessionService;
        _serviceManagementService = serviceManagementService;
        _accountService = accountService;
    }

    public async Task<bool> TryHandleAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        string text,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionService.GetAsync(telegramUserId, cancellationToken);
        if (session?.MasterId is not { } masterId)
        {
            return false;
        }

        if (session.Role != TelegramUserRole.Master)
        {
            if (MasterPanelKeyboardBuilder.IsPanelActionButton(text) ||
                MasterPanelKeyboardBuilder.IsAccountButton(text))
            {
                await botClient.SendMessage(
                    chatId,
                    TelegramBotTexts.MasterPanel.ClientsOnly,
                    replyMarkup: MenuKeyboardBuilder.Build(session.Role),
                    cancellationToken: cancellationToken);
                return true;
            }

            return false;
        }

        if (session.CurrentStep == ConversationStep.MasterConfirmingDeleteAccount)
        {
            await HandleDeleteConfirmationAsync(botClient, chatId, telegramUserId, text, cancellationToken);
            return true;
        }

        if (IsAddingServiceStep(session.CurrentStep))
        {
            await HandleAddServiceStepAsync(botClient, chatId, telegramUserId, masterId, session, text, cancellationToken);
            return true;
        }

        if (text == TelegramBotTexts.MasterPanel.ButtonLogout)
        {
            await LogoutAsync(botClient, chatId, telegramUserId, cancellationToken);
            return true;
        }

        if (text == TelegramBotTexts.MasterPanel.ButtonDeleteAccount)
        {
            await StartDeleteAccountConfirmationAsync(botClient, chatId, telegramUserId, cancellationToken);
            return true;
        }

        if (IsAtMasterMainMenu(session.CurrentStep) &&
            MasterPanelKeyboardBuilder.IsPanelActionButton(text))
        {
            return await HandlePanelMenuAsync(botClient, chatId, telegramUserId, masterId, session, text, cancellationToken);
        }

        return false;
    }

    private async Task<bool> HandlePanelMenuAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        int masterId,
        TelegramUserSession session,
        string text,
        CancellationToken cancellationToken)
    {
        switch (text)
        {
            case TelegramBotTexts.MasterPanel.ButtonMyServices:
                await ShowServicesAsync(botClient, chatId, masterId, session.Role, cancellationToken);
                return true;

            case TelegramBotTexts.MasterPanel.ButtonAddService:
                await StartAddServiceAsync(botClient, chatId, telegramUserId, cancellationToken);
                return true;

            default:
                return false;
        }
    }

    private async Task HandleAddServiceStepAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        int masterId,
        TelegramUserSession session,
        string text,
        CancellationToken cancellationToken)
    {
        var draft = _sessionService.GetMasterServiceDraft(session) ?? new MasterServiceDraft();

        switch (session.CurrentStep)
        {
            case ConversationStep.MasterAddingServiceName:
                if (!TryParseServiceName(text, out var name))
                {
                    await botClient.SendMessage(
                        chatId,
                        TelegramBotTexts.MasterPanel.InvalidServiceName,
                        cancellationToken: cancellationToken);
                    return;
                }

                draft.ServiceName = name;
                await _sessionService.SaveMasterServiceDraftAsync(
                    telegramUserId,
                    chatId,
                    draft,
                    ConversationStep.MasterAddingServiceDuration,
                    cancellationToken);

                await botClient.SendMessage(
                    chatId,
                    TelegramBotTexts.MasterPanel.PromptServiceDuration,
                    cancellationToken: cancellationToken);
                return;

            case ConversationStep.MasterAddingServiceDuration:
                if (!int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var duration) ||
                    duration is < 1 or > 1440)
                {
                    await botClient.SendMessage(
                        chatId,
                        TelegramBotTexts.MasterPanel.InvalidDuration,
                        cancellationToken: cancellationToken);
                    return;
                }

                draft.DurationInMinutes = duration;
                await _sessionService.SaveMasterServiceDraftAsync(
                    telegramUserId,
                    chatId,
                    draft,
                    ConversationStep.MasterAddingServicePrice,
                    cancellationToken);

                await botClient.SendMessage(
                    chatId,
                    TelegramBotTexts.MasterPanel.PromptServicePrice,
                    cancellationToken: cancellationToken);
                return;

            case ConversationStep.MasterAddingServicePrice:
                if (!TryParsePrice(text, out var price))
                {
                    await botClient.SendMessage(
                        chatId,
                        TelegramBotTexts.MasterPanel.InvalidPrice,
                        cancellationToken: cancellationToken);
                    return;
                }

                draft.Price = price;
                var result = await _serviceManagementService.CreateForMasterAsync(masterId, draft, cancellationToken);

                if (!result.Success)
                {
                    await botClient.SendMessage(
                        chatId,
                        result.ErrorMessage!,
                        cancellationToken: cancellationToken);
                    return;
                }

                var created = result.Service!;
                var sessionAfterSave = await _sessionService.ReturnToMainMenuAsync(
                    telegramUserId,
                    chatId,
                    cancellationToken);

                await botClient.SendMessage(
                    chatId,
                    TelegramBotTexts.MasterPanel.ServiceCreated(
                        created.ServiceName,
                        created.DurationInMinutes,
                        created.Price),
                    replyMarkup: MenuKeyboardBuilder.Build(sessionAfterSave.Role),
                    cancellationToken: cancellationToken);
                return;
        }
    }

    private async Task StartAddServiceAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        CancellationToken cancellationToken)
    {
        await _sessionService.SaveMasterServiceDraftAsync(
            telegramUserId,
            chatId,
            new MasterServiceDraft(),
            ConversationStep.MasterAddingServiceName,
            cancellationToken);

        await botClient.SendMessage(
            chatId,
            TelegramBotTexts.MasterPanel.PromptServiceName,
            cancellationToken: cancellationToken);
    }

    private async Task ShowServicesAsync(
        ITelegramBotClient botClient,
        long chatId,
        int masterId,
        TelegramUserRole role,
        CancellationToken cancellationToken)
    {
        var services = await _serviceManagementService.GetByMasterIdAsync(masterId, cancellationToken);
        var message = TelegramBotTexts.MasterPanel.FormatServicesList(services);

        await botClient.SendMessage(
            chatId,
            message,
            replyMarkup: MenuKeyboardBuilder.BuildMasterMenu(),
            cancellationToken: cancellationToken);
    }

    private async Task LogoutAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        CancellationToken cancellationToken)
    {
        await _accountService.LogoutAsync(telegramUserId, chatId, cancellationToken);

        await botClient.SendMessage(
            chatId,
            TelegramBotTexts.MasterPanel.LoggedOut,
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken);
    }

    private async Task StartDeleteAccountConfirmationAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        CancellationToken cancellationToken)
    {
        await _sessionService.SetStepAsync(
            telegramUserId,
            chatId,
            ConversationStep.MasterConfirmingDeleteAccount,
            cancellationToken);

        await botClient.SendMessage(
            chatId,
            TelegramBotTexts.MasterPanel.DeleteAccountWarning,
            replyMarkup: MasterPanelKeyboardBuilder.BuildDeleteConfirmation(),
            cancellationToken: cancellationToken);
    }

    private async Task HandleDeleteConfirmationAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        string text,
        CancellationToken cancellationToken)
    {
        if (MasterPanelKeyboardBuilder.IsCancelDelete(text))
        {
            await _sessionService.ReturnToMainMenuAsync(telegramUserId, chatId, cancellationToken);

            await botClient.SendMessage(
                chatId,
                TelegramBotTexts.MasterPanel.DeleteAccountCancelled,
                replyMarkup: MenuKeyboardBuilder.BuildMasterMenu(),
                cancellationToken: cancellationToken);
            return;
        }

        if (!MasterPanelKeyboardBuilder.IsConfirmDelete(text))
        {
            await botClient.SendMessage(
                chatId,
                TelegramBotTexts.MasterPanel.DeleteAccountWarning,
                replyMarkup: MasterPanelKeyboardBuilder.BuildDeleteConfirmation(),
                cancellationToken: cancellationToken);
            return;
        }

        var result = await _accountService.DeleteAccountAsync(telegramUserId, cancellationToken);
        if (!result.Success)
        {
            await botClient.SendMessage(
                chatId,
                result.ErrorMessage ?? TelegramBotTexts.Common.UnexpectedError,
                cancellationToken: cancellationToken);
            return;
        }

        await botClient.SendMessage(
            chatId,
            TelegramBotTexts.MasterPanel.DeleteAccountSuccess,
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken);
    }

    private static bool IsAtMasterMainMenu(ConversationStep step) =>
        step is ConversationStep.Idle or ConversationStep.MasterPanelMenu;

    private static bool IsAddingServiceStep(ConversationStep step) =>
        step is ConversationStep.MasterAddingServiceName
            or ConversationStep.MasterAddingServiceDuration
            or ConversationStep.MasterAddingServicePrice;

    private static bool TryParseServiceName(string text, out string name)
    {
        name = text.Trim();
        return name.Length is >= 2 and <= 200;
    }

    private static bool TryParsePrice(string text, out decimal price)
    {
        var normalized = text.Trim().Replace(',', '.');
        if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out price))
        {
            return false;
        }

        return price is >= 0 and <= 999999.99m;
    }
}
