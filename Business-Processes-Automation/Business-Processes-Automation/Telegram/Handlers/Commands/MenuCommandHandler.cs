using Business_Processes_Automation.BLL.Services;
using Business_Processes_Automation.Telegram.Abstractions;
using Business_Processes_Automation.Telegram.Keyboards;
using Business_Processes_Automation.Telegram.Localization;
using Telegram.Bot;

namespace Business_Processes_Automation.Telegram.Handlers.Commands;

public class MenuCommandHandler(ITelegramUserSessionService sessionService) : ITelegramCommandHandler
{
    public bool CanHandle(string messageText) =>
        messageText.Equals("/menu", StringComparison.OrdinalIgnoreCase);

    public async Task HandleAsync(TelegramCommandContext context, CancellationToken cancellationToken = default)
    {
        if (context.TelegramUserId is not { } telegramUserId)
        {
            return;
        }

        var session = await sessionService.GetAsync(telegramUserId, cancellationToken);

        if (session?.MasterId is null)
        {
            await context.BotClient.SendMessage(
                context.ChatId,
                TelegramBotTexts.Common.SessionRequired,
                cancellationToken: cancellationToken);
            return;
        }

        var keyboard = MenuKeyboardBuilder.Build(session.Role);

        await context.BotClient.SendMessage(
            context.ChatId,
            TelegramBotTexts.Menu.Title,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }
}
