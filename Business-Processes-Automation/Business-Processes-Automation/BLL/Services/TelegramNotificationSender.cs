using Business_Processes_Automation.BLL.Helpers;
using Business_Processes_Automation.BLL.Interfaces;
using Telegram.Bot;

namespace Business_Processes_Automation.BLL.Services;

public class TelegramNotificationSender : ITelegramNotificationSender
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<TelegramNotificationSender> _logger;

    public TelegramNotificationSender(
        ITelegramBotClient botClient,
        ILogger<TelegramNotificationSender> logger)
    {
        _botClient = botClient;
        _logger = logger;
    }

    public async Task<bool> TrySendAsync(
        long chatId,
        string text,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var parts = TelegramMessageSplitter.Split(text);

            foreach (var part in parts)
            {
                await _botClient.SendMessage(
                    chatId,
                    part,
                    cancellationToken: cancellationToken);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to send Telegram notification to chat {ChatId}",
                chatId);

            return false;
        }
    }
}
