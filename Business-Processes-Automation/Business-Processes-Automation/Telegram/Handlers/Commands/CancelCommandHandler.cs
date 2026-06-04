using Business_Processes_Automation.BLL.Services;
using Business_Processes_Automation.DAL.Enums;
using Business_Processes_Automation.Telegram.Abstractions;
using Business_Processes_Automation.Telegram.Keyboards;
using Business_Processes_Automation.Telegram.Localization;
using Telegram.Bot;

namespace Business_Processes_Automation.Telegram.Handlers.Commands;

public class CancelCommandHandler : ITelegramCommandHandler
{
    private readonly ITelegramUserSessionService _sessionService;

    public CancelCommandHandler(ITelegramUserSessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public bool CanHandle(string messageText) =>
        messageText.Equals("/cancel", StringComparison.OrdinalIgnoreCase);

    public async Task HandleAsync(TelegramCommandContext context, CancellationToken cancellationToken = default)
    {
        if (context.TelegramUserId is not { } telegramUserId)
        {
            return;
        }

        var session = await _sessionService.ReturnToMainMenuAsync(telegramUserId, context.ChatId, cancellationToken);

        if (session.MasterId is null)
        {
            await context.BotClient.SendMessage(
                context.ChatId,
                TelegramBotTexts.Common.SessionRequired,
                cancellationToken: cancellationToken);
            return;
        }

        var message = session.Role == TelegramUserRole.Master
            ? TelegramBotTexts.Menu.CancelConfirmedMaster
            : TelegramBotTexts.Menu.CancelConfirmed;

        await context.BotClient.SendMessage(
            context.ChatId,
            message,
            replyMarkup: MenuKeyboardBuilder.Build(session.Role),
            cancellationToken: cancellationToken);
    }
}
