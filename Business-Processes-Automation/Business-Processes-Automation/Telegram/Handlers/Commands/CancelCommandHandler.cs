using Business_Processes_Automation.BLL.Services;
using Business_Processes_Automation.Telegram.Abstractions;
using Business_Processes_Automation.Telegram.Keyboards;
using Business_Processes_Automation.Telegram.Localization;
using Telegram.Bot;

namespace Business_Processes_Automation.Telegram.Handlers.Commands;

public class CancelCommandHandler(ITelegramUserSessionService sessionService) : ITelegramCommandHandler
{
    public bool CanHandle(string messageText) =>
        messageText.Equals("/cancel", StringComparison.OrdinalIgnoreCase);

    public async Task HandleAsync(TelegramCommandContext context, CancellationToken cancellationToken = default)
    {
        if (context.TelegramUserId is not { } telegramUserId)
        {
            return;
        }

        var session = await sessionService.ResetStepAsync(telegramUserId, context.ChatId, cancellationToken);

        if (session.MasterId is null)
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
            TelegramBotTexts.Menu.CancelConfirmed,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }
}
