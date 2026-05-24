using Business_Processes_Automation.Telegram.Abstractions;
using Business_Processes_Automation.Telegram.Handlers.Messages;
using Business_Processes_Automation.Telegram.Localization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Business_Processes_Automation.Telegram.Handlers;

public class TelegramUpdateHandler : ITelegramUpdateHandler
{
    private readonly MessageUpdateHandler _messageUpdateHandler;
    private readonly ILogger<TelegramUpdateHandler> _logger;

    public TelegramUpdateHandler(
        MessageUpdateHandler messageUpdateHandler,
        ILogger<TelegramUpdateHandler> logger)
    {
        _messageUpdateHandler = messageUpdateHandler;
        _logger = logger;
    }

    public async Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Telegram update {UpdateId}, type {UpdateType}",
            update.Id,
            update.Type);

        try
        {
            switch (update.Type)
            {
                case UpdateType.Message when update.Message is { } message:
                    await _messageUpdateHandler.HandleAsync(botClient, message, cancellationToken);
                    return;
                default:
                    _logger.LogDebug("Update {UpdateId} skipped: unsupported type", update.Id);
                    return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle Telegram update {UpdateId}", update.Id);
            await TryNotifyUserAsync(botClient, update, cancellationToken);
            throw;
        }
    }

    private static async Task TryNotifyUserAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken)
    {
        var chatId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message?.Chat.Id;
        if (chatId is null)
        {
            return;
        }

        try
        {
            await botClient.SendMessage(
                chatId.Value,
                TelegramBotTexts.Common.UnexpectedError,
                cancellationToken: cancellationToken);
        }
        catch
        {
            // Suppress secondary failures while reporting the original error.
        }
    }
}
