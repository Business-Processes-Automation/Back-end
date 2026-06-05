using Business_Processes_Automation.Telegram.Handlers.Commands;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Business_Processes_Automation.Telegram.Handlers.Messages;

public class MessageUpdateHandler
{
    private readonly TelegramCommandDispatcher _commandDispatcher;
    private readonly MasterTelegramLinkHandler _masterTelegramLinkHandler;
    private readonly MasterScheduleHandler _masterScheduleHandler;
    private readonly MasterPanelHandler _masterPanelHandler;
    private readonly ClientBookingHandler _clientBookingHandler;
    private readonly MenuReplyHandler _menuReplyHandler;
    private readonly ILogger<MessageUpdateHandler> _logger;

    public MessageUpdateHandler(
        TelegramCommandDispatcher commandDispatcher,
        MasterTelegramLinkHandler masterTelegramLinkHandler,
        MasterScheduleHandler masterScheduleHandler,
        MasterPanelHandler masterPanelHandler,
        ClientBookingHandler clientBookingHandler,
        MenuReplyHandler menuReplyHandler,
        ILogger<MessageUpdateHandler> logger)
    {
        _commandDispatcher = commandDispatcher;
        _masterTelegramLinkHandler = masterTelegramLinkHandler;
        _masterScheduleHandler = masterScheduleHandler;
        _masterPanelHandler = masterPanelHandler;
        _clientBookingHandler = clientBookingHandler;
        _menuReplyHandler = menuReplyHandler;
        _logger = logger;
    }

    public async Task HandleAsync(
        ITelegramBotClient botClient,
        Message message,
        CancellationToken cancellationToken = default)
    {
        if (message.Text is not { } text)
        {
            _logger.LogDebug("Message {MessageId} has no text", message.MessageId);
            return;
        }

        _logger.LogInformation(
            "Message from chat {ChatId}, user {UserId}: {Text}",
            message.Chat.Id,
            message.From?.Id,
            text);

        if (await _commandDispatcher.TryDispatchAsync(botClient, message, cancellationToken))
        {
            return;
        }

        if (message.From?.Id is { } telegramUserId)
        {
            if (await _masterTelegramLinkHandler.TryHandleAsync(
                    botClient, message, telegramUserId, text, cancellationToken))
            {
                return;
            }

            if (await _clientBookingHandler.TryHandleAsync(
                    botClient, message, telegramUserId, text, cancellationToken))
            {
                return;
            }

            if (await _masterScheduleHandler.TryHandleAsync(
                    botClient, message.Chat.Id, telegramUserId, text, cancellationToken))
            {
                return;
            }

            if (await _masterPanelHandler.TryHandleAsync(
                    botClient, message.Chat.Id, telegramUserId, text, cancellationToken))
            {
                return;
            }

            if (await _menuReplyHandler.TryHandleAsync(
                    botClient, message.Chat.Id, telegramUserId, text, cancellationToken))
            {
                return;
            }
        }

        _logger.LogDebug("No handler matched message in chat {ChatId}", message.Chat.Id);
    }
}
