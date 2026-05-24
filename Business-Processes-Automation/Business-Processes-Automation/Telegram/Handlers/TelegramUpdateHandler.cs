using Business_Processes_Automation.Telegram.Abstractions;
using Business_Processes_Automation.Telegram.Handlers.Messages;
using Business_Processes_Automation.Telegram.Localization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Business_Processes_Automation.Telegram.Handlers;

public class TelegramUpdateHandler(
    MessageUpdateHandler messageUpdateHandler,
    ILogger<TelegramUpdateHandler> logger) : ITelegramUpdateHandler
{
    public async Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Telegram update {UpdateId}, type {UpdateType}",
            update.Id,
            update.Type);

        try
        {
            switch (update.Type)
            {
                case UpdateType.Message when update.Message is { } message:
                    await messageUpdateHandler.HandleAsync(botClient, message, cancellationToken);
                    return;
                default:
                    logger.LogDebug("Update {UpdateId} skipped: unsupported type", update.Id);
                    return;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to handle Telegram update {UpdateId}", update.Id);
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
