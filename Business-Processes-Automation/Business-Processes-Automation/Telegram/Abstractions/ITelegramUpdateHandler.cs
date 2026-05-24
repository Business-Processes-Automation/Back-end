using Telegram.Bot;
using Telegram.Bot.Types;

namespace Business_Processes_Automation.Telegram.Abstractions;

public interface ITelegramUpdateHandler
{
    Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken = default);
}
