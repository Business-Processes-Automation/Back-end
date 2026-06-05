using Business_Processes_Automation.BLL.Interfaces;
using Business_Processes_Automation.BLL.Services;
using Business_Processes_Automation.DAL.Enums;
using Business_Processes_Automation.Telegram.Keyboards;
using Business_Processes_Automation.Telegram.Localization;
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

        if (text == TelegramBotTexts.MasterPanel.ButtonLogout)
        {
            await LogoutAsync(botClient, chatId, telegramUserId, cancellationToken);
            return true;
        }

        if (IsAtMasterMainMenu(session.CurrentStep) &&
            MasterPanelKeyboardBuilder.IsPanelActionButton(text))
        {
            return await HandlePanelMenuAsync(botClient, chatId, masterId, text, cancellationToken);
        }

        return false;
    }

    private async Task<bool> HandlePanelMenuAsync(
        ITelegramBotClient botClient,
        long chatId,
        int masterId,
        string text,
        CancellationToken cancellationToken)
    {
        if (text != TelegramBotTexts.MasterPanel.ButtonMyServices)
        {
            return false;
        }

        await ShowServicesAsync(botClient, chatId, masterId, cancellationToken);
        return true;
    }

    private async Task ShowServicesAsync(
        ITelegramBotClient botClient,
        long chatId,
        int masterId,
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

    private static bool IsAtMasterMainMenu(ConversationStep step) =>
        step is ConversationStep.Idle;
}
