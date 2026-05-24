using Business_Processes_Automation.BLL.Services;
using Business_Processes_Automation.DAL.Enums;
using Business_Processes_Automation.Telegram.Abstractions;
using Business_Processes_Automation.Telegram.Keyboards;
using Business_Processes_Automation.Telegram.Localization;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Business_Processes_Automation.Telegram.Handlers.Commands;

public class StartCommandHandler(
    IMasterService masterService,
    ITelegramUserSessionService sessionService,
    ILogger<StartCommandHandler> logger) : ITelegramCommandHandler
{
    public bool CanHandle(string messageText) =>
        messageText.StartsWith("/start", StringComparison.OrdinalIgnoreCase);

    public async Task HandleAsync(TelegramCommandContext context, CancellationToken cancellationToken = default)
    {
        if (context.TelegramUserId is not { } telegramUserId)
        {
            logger.LogWarning("Received /start without Telegram user id in chat {ChatId}", context.ChatId);
            return;
        }

        if (context.StartPayload is not { } payload)
        {
            await context.BotClient.SendMessage(
                context.ChatId,
                TelegramBotTexts.Start.MissingStartParameter,
                cancellationToken: cancellationToken);
            return;
        }

        logger.LogInformation(
            "Received /start with payload {StartPayload} from user {TelegramUserId}",
            payload,
            telegramUserId);

        var masterTelegram = await masterService.GetByBotStartParameterAsync(payload, cancellationToken);

        if (masterTelegram?.Master is not { } master)
        {
            await context.BotClient.SendMessage(
                context.ChatId,
                TelegramBotTexts.Start.MasterNotFound,
                cancellationToken: cancellationToken);
            return;
        }

        var role = telegramUserId == masterTelegram.TelegramUserId
            ? TelegramUserRole.Master
            : TelegramUserRole.Client;

        await sessionService.BindMasterAsync(
            telegramUserId,
            context.ChatId,
            master.Id,
            role,
            cancellationToken);

        var welcomeText = TelegramBotTexts.Start.WelcomeToMaster(masterService.GetDisplayName(master));
        ReplyKeyboardMarkup keyboard = MenuKeyboardBuilder.Build(role);

        await context.BotClient.SendMessage(
            context.ChatId,
            welcomeText,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        logger.LogInformation(
            "Bound user {TelegramUserId} to master {MasterId} with role {Role}",
            telegramUserId,
            master.Id,
            role);
    }
}
