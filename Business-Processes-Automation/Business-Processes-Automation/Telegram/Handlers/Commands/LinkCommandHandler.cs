using Business_Processes_Automation.Telegram.Abstractions;
using Business_Processes_Automation.Telegram.Handlers.Messages;

namespace Business_Processes_Automation.Telegram.Handlers.Commands;

public class LinkCommandHandler : ITelegramCommandHandler
{
    private readonly MasterTelegramLinkHandler _linkHandler;

    public LinkCommandHandler(MasterTelegramLinkHandler linkHandler)
    {
        _linkHandler = linkHandler;
    }

    public bool CanHandle(string messageText) =>
        messageText.StartsWith("/link", StringComparison.OrdinalIgnoreCase) ||
        messageText.StartsWith("/register", StringComparison.OrdinalIgnoreCase);

    public Task HandleAsync(TelegramCommandContext context, CancellationToken cancellationToken = default) =>
        _linkHandler.StartLinkingAsync(
            context.BotClient,
            context.Message,
            cancellationToken);
}
