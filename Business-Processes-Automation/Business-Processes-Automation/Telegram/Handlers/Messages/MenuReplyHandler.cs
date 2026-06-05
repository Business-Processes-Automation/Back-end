using Business_Processes_Automation.BLL.Interfaces;
using Business_Processes_Automation.BLL.Services;
using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;
using Business_Processes_Automation.Telegram.Keyboards;
using Business_Processes_Automation.Telegram.Localization;
using Telegram.Bot;

namespace Business_Processes_Automation.Telegram.Handlers.Messages;

public class MenuReplyHandler
{
    private readonly ITelegramUserSessionService _sessionService;
    private readonly IMasterService _masterService;
    private readonly IServiceManagementService _serviceManagementService;
    private readonly IClientAppointmentsService _clientAppointmentsService;

    public MenuReplyHandler(
        ITelegramUserSessionService sessionService,
        IMasterService masterService,
        IServiceManagementService serviceManagementService,
        IClientAppointmentsService clientAppointmentsService)
    {
        _sessionService = sessionService;
        _masterService = masterService;
        _serviceManagementService = serviceManagementService;
        _clientAppointmentsService = clientAppointmentsService;
    }

    public async Task<bool> TryHandleAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        string text,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionService.GetAsync(telegramUserId, cancellationToken);

        if (session?.MasterId is null)
        {
            return false;
        }

        if (MenuKeyboardBuilder.IsBackToMenu(text))
        {
            await ShowMainMenuAsync(botClient, chatId, telegramUserId, session.Role, cancellationToken);
            return true;
        }

        if (session.Role == TelegramUserRole.Master)
        {
            if (MenuKeyboardBuilder.IsClientMenuButton(text) ||
                text == TelegramBotTexts.Menu.ButtonMasterPanel)
            {
                await ShowMainMenuAsync(botClient, chatId, telegramUserId, session.Role, cancellationToken);
                return true;
            }

            return false;
        }

        if (!MenuKeyboardBuilder.IsClientMenuButton(text))
        {
            return false;
        }

        return text switch
        {
            TelegramBotTexts.Menu.ButtonServices =>
                await ShowServicesAsync(botClient, chatId, telegramUserId, session, cancellationToken),

            TelegramBotTexts.Menu.ButtonBook => false,

            TelegramBotTexts.Menu.ButtonMyAppointments =>
                await ShowMyAppointmentsAsync(
                    botClient,
                    chatId,
                    telegramUserId,
                    session,
                    cancellationToken),

            TelegramBotTexts.Menu.ButtonAboutMaster =>
                await ShowAboutMasterAsync(botClient, chatId, telegramUserId, session, cancellationToken),

            _ => false
        };
    }

    private async Task<bool> ShowServicesAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        TelegramUserSession session,
        CancellationToken cancellationToken)
    {
        var services = await _serviceManagementService.GetByMasterIdAsync(
            session.MasterId!.Value,
            cancellationToken);
        var message = TelegramBotTexts.Menu.FormatServicesList(services);

        await SendTextAsync(botClient, chatId, telegramUserId, session.Role, message, cancellationToken);
        return true;
    }

    private async Task<bool> ShowMyAppointmentsAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        TelegramUserSession session,
        CancellationToken cancellationToken)
    {
        var message = await _clientAppointmentsService.BuildMyAppointmentsMessageAsync(
            telegramUserId,
            session.MasterId!.Value,
            cancellationToken);

        await SendTextAsync(botClient, chatId, telegramUserId, session.Role, message, cancellationToken);
        return true;
    }

    private async Task<bool> ShowAboutMasterAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        TelegramUserSession session,
        CancellationToken cancellationToken)
    {
        var master = await _masterService.GetByIdAsync(session.MasterId!.Value, cancellationToken);

        if (master is null)
        {
            await botClient.SendMessage(
                chatId,
                TelegramBotTexts.Start.MasterNotFound,
                cancellationToken: cancellationToken);
            return true;
        }

        var message = TelegramBotTexts.Menu.AboutMaster(master, _masterService.GetDisplayName(master));

        await SendTextAsync(botClient, chatId, telegramUserId, session.Role, message, cancellationToken);
        return true;
    }

    private async Task ShowMainMenuAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        TelegramUserRole role,
        CancellationToken cancellationToken)
    {
        await _sessionService.ReturnToMainMenuAsync(telegramUserId, chatId, cancellationToken);

        await botClient.SendMessage(
            chatId,
            MenuKeyboardBuilder.GetMenuTitle(role),
            replyMarkup: MenuKeyboardBuilder.Build(role),
            cancellationToken: cancellationToken);
    }

    private async Task<bool> SendTextAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        TelegramUserRole role,
        string message,
        CancellationToken cancellationToken)
    {
        await _sessionService.ReturnToMainMenuAsync(telegramUserId, chatId, cancellationToken);

        await botClient.SendMessage(
            chatId,
            message,
            replyMarkup: MenuKeyboardBuilder.Build(role),
            cancellationToken: cancellationToken);

        return true;
    }
}
