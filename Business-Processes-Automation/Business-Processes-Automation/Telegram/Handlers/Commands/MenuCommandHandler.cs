using Business_Processes_Automation.BLL.Services;
using Business_Processes_Automation.Telegram.Abstractions;
using Business_Processes_Automation.Telegram.Keyboards;
using Business_Processes_Automation.Telegram.Localization;
using Telegram.Bot;

namespace Business_Processes_Automation.Telegram.Handlers.Commands;

public class MenuCommandHandler : ITelegramCommandHandler
{
    private readonly ITelegramUserSessionService _sessionService;

    public MenuCommandHandler(ITelegramUserSessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public bool CanHandle(string messageText) =>
        messageText.Equals("/menu", StringComparison.OrdinalIgnoreCase);

    public async Task HandleAsync(TelegramCommandContext context, CancellationToken cancellationToken = default)
    {
        if (context.TelegramUserId is not { } telegramUserId)
        {
            return;
        }

        var session = await _sessionService.GetAsync(telegramUserId, cancellationToken);

        if (session?.MasterId is null)
        {
            await context.BotClient.SendMessage(
                context.ChatId,
                TelegramBotTexts.Common.SessionRequired,
                cancellationToken: cancellationToken);
            return;
        }

        await _sessionService.ReturnToMainMenuAsync(telegramUserId, context.ChatId, cancellationToken);

        await context.BotClient.SendMessage(
            context.ChatId,
            MenuKeyboardBuilder.GetMenuTitle(session.Role),
            replyMarkup: MenuKeyboardBuilder.Build(session.Role),
            cancellationToken: cancellationToken);
    }
}
