using Business_Processes_Automation.Telegram.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Business_Processes_Automation.Telegram.Handlers.Commands;

public class TelegramCommandDispatcher
{
    private readonly IReadOnlyList<ITelegramCommandHandler> _handlers;

    public TelegramCommandDispatcher(IEnumerable<ITelegramCommandHandler> handlers)
    {
        _handlers = handlers.ToList();
    }

    public async Task<bool> TryDispatchAsync(
        ITelegramBotClient botClient,
        Message message,
        CancellationToken cancellationToken = default)
    {
        if (message.Text is not { } text)
        {
            return false;
        }

        var context = new TelegramCommandContext(botClient, message);

        foreach (var handler in _handlers)
        {
            if (!handler.CanHandle(text))
            {
                continue;
            }

            await handler.HandleAsync(context, cancellationToken);
            return true;
        }

        return false;
    }
}
