using Business_Processes_Automation.Telegram.Abstractions;
using Business_Processes_Automation.Telegram.Handlers.Messages;

namespace Business_Processes_Automation.Telegram.Handlers.Commands;

public class RegisterCommandHandler : ITelegramCommandHandler
{
    private readonly MasterRegistrationHandler _registrationHandler;

    public RegisterCommandHandler(MasterRegistrationHandler registrationHandler)
    {
        _registrationHandler = registrationHandler;
    }

    public bool CanHandle(string messageText) =>
        messageText.StartsWith("/register", StringComparison.OrdinalIgnoreCase);

    public Task HandleAsync(TelegramCommandContext context, CancellationToken cancellationToken = default) =>
        _registrationHandler.StartRegistrationAsync(
            context.BotClient,
            context.Message,
            cancellationToken);
}
