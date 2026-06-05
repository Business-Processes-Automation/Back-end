using Business_Processes_Automation.BLL.Services;
using Business_Processes_Automation.DAL.Enums;
using Business_Processes_Automation.Telegram.Abstractions;
using Business_Processes_Automation.Telegram.Handlers.Messages;
using Business_Processes_Automation.Telegram.Keyboards;
using Business_Processes_Automation.Telegram.Localization;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Business_Processes_Automation.Telegram.Handlers.Commands;

public class StartCommandHandler : ITelegramCommandHandler
{
    private readonly IMasterService _masterService;
    private readonly IMasterTelegramLinkService _linkService;
    private readonly ITelegramUserSessionService _sessionService;
    private readonly ILogger<StartCommandHandler> _logger;

    public StartCommandHandler(
        IMasterService masterService,
        IMasterTelegramLinkService linkService,
        ITelegramUserSessionService sessionService,
        ILogger<StartCommandHandler> logger)
    {
        _masterService = masterService;
        _linkService = linkService;
        _sessionService = sessionService;
        _logger = logger;
    }

    public bool CanHandle(string messageText) =>
        messageText.StartsWith("/start", StringComparison.OrdinalIgnoreCase);

    public async Task HandleAsync(TelegramCommandContext context, CancellationToken cancellationToken = default)
    {
        if (context.TelegramUserId is not { } telegramUserId)
        {
            _logger.LogWarning("Received /start without Telegram user id in chat {ChatId}", context.ChatId);
            return;
        }

        if (context.StartPayload is null)
        {
            var ownMaster = await _linkService.GetByTelegramUserIdAsync(telegramUserId, cancellationToken);
            if (ownMaster?.Master is { } registeredMaster)
            {
                await BindAndWelcomeAsync(
                    context,
                    telegramUserId,
                    registeredMaster,
                    TelegramUserRole.Master,
                    TelegramBotTexts.Start.WelcomeRegisteredMaster(_masterService.GetDisplayName(registeredMaster)),
                    cancellationToken);
                return;
            }

            await context.BotClient.SendMessage(
                context.ChatId,
                TelegramBotTexts.Start.MissingStartParameter,
                cancellationToken: cancellationToken);
            return;
        }

        var payload = context.StartPayload;

        _logger.LogInformation(
            "Received /start with payload {StartPayload} from user {TelegramUserId}",
            payload,
            telegramUserId);

        var masterTelegram = await _masterService.GetByBotStartParameterAsync(payload, cancellationToken);

        if (masterTelegram?.Master is not { } master)
        {
            await context.BotClient.SendMessage(
                context.ChatId,
                TelegramBotTexts.Start.MasterNotFound,
                cancellationToken: cancellationToken);
            return;
        }

        var role = masterTelegram.TelegramUserId > 0 && telegramUserId == masterTelegram.TelegramUserId
            ? TelegramUserRole.Master
            : TelegramUserRole.Client;

        var displayName = _masterService.GetDisplayName(master);
        var welcomeText = role == TelegramUserRole.Master
            ? TelegramBotTexts.Start.WelcomeRegisteredMaster(displayName)
            : TelegramBotTexts.Start.WelcomeToMaster(displayName);

        await BindAndWelcomeAsync(
            context,
            telegramUserId,
            master,
            role,
            welcomeText,
            cancellationToken);

        _logger.LogInformation(
            "Bound user {TelegramUserId} to master {MasterId} with role {Role}",
            telegramUserId,
            master.Id,
            role);
    }

    private async Task BindAndWelcomeAsync(
        TelegramCommandContext context,
        long telegramUserId,
        DAL.Entities.Master master,
        TelegramUserRole role,
        string welcomeText,
        CancellationToken cancellationToken)
    {
        await _sessionService.BindMasterAsync(
            telegramUserId,
            context.ChatId,
            master.Id,
            role,
            cancellationToken);

        ReplyKeyboardMarkup keyboard = MenuKeyboardBuilder.Build(role);

        await context.BotClient.SendMessage(
            context.ChatId,
            welcomeText,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }
}
