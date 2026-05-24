using Telegram.Bot;
using Telegram.Bot.Types;

namespace Business_Processes_Automation.Telegram.Handlers.Commands;

public sealed class TelegramCommandContext
{
    public TelegramCommandContext(ITelegramBotClient botClient, Message message)
    {
        BotClient = botClient;
        Message = message;
    }

    public ITelegramBotClient BotClient { get; }

    public Message Message { get; }

    public long ChatId => Message.Chat.Id;

    public long? TelegramUserId => Message.From?.Id;

    public string Text => Message.Text ?? string.Empty;

    /// <summary>
    /// Deep-link payload from /start {payload}, e.g. anna_nails.
    /// </summary>
    public string? StartPayload
    {
        get
        {
            var parts = Text.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return parts.Length > 1 ? parts[1] : null;
        }
    }
}
