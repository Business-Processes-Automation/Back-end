using Business_Processes_Automation.Telegram.Handlers.Commands;

namespace Business_Processes_Automation.Telegram.Abstractions;

public interface ITelegramCommandHandler
{
    bool CanHandle(string messageText);

    Task HandleAsync(TelegramCommandContext context, CancellationToken cancellationToken = default);
}
