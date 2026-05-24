using Business_Processes_Automation.Telegram.Handlers.Commands;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Business_Processes_Automation.Telegram.Handlers.Messages;

public class MessageUpdateHandler(
    TelegramCommandDispatcher commandDispatcher,
    MenuReplyHandler menuReplyHandler,
    ILogger<MessageUpdateHandler> logger)
{
    public async Task HandleAsync(
        ITelegramBotClient botClient,
        Message message,
        CancellationToken cancellationToken = default)
    {
        if (message.Text is not { } text)
        {
            logger.LogDebug("Message {MessageId} has no text", message.MessageId);
            return;
        }

        logger.LogInformation(
            "Message from chat {ChatId}, user {UserId}: {Text}",
            message.Chat.Id,
            message.From?.Id,
            text);

        if (await commandDispatcher.TryDispatchAsync(botClient, message, cancellationToken))
        {
            return;
        }

        if (message.From?.Id is { } telegramUserId &&
            await menuReplyHandler.TryHandleAsync(botClient, message.Chat.Id, telegramUserId, text, cancellationToken))
        {
            return;
        }

        logger.LogDebug("No handler matched message in chat {ChatId}", message.Chat.Id);
    }
}
