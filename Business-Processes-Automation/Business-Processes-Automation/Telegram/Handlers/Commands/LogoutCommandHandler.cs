using Business_Processes_Automation.BLL.Services;
using Business_Processes_Automation.Telegram.Abstractions;
using Business_Processes_Automation.Telegram.Localization;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Business_Processes_Automation.Telegram.Handlers.Commands;

public class LogoutCommandHandler : ITelegramCommandHandler
{
    private readonly IMasterAccountService _accountService;
    private readonly IMasterTelegramLinkService _linkService;

    public LogoutCommandHandler(
        IMasterAccountService accountService,
        IMasterTelegramLinkService linkService)
    {
        _accountService = accountService;
        _linkService = linkService;
    }

    public bool CanHandle(string messageText) =>
        messageText.StartsWith("/logout", StringComparison.OrdinalIgnoreCase);

    public async Task HandleAsync(TelegramCommandContext context, CancellationToken cancellationToken = default)
    {
        if (context.TelegramUserId is not { } telegramUserId)
        {
            return;
        }

        var masterTelegram = await _linkService.GetByTelegramUserIdAsync(telegramUserId, cancellationToken);
        if (masterTelegram is null)
        {
            await context.BotClient.SendMessage(
                context.ChatId,
                TelegramBotTexts.MasterPanel.MasterAccountNotFound,
                cancellationToken: cancellationToken);
            return;
        }

        await _accountService.LogoutAsync(telegramUserId, context.ChatId, cancellationToken);

        await context.BotClient.SendMessage(
            context.ChatId,
            TelegramBotTexts.MasterPanel.LoggedOut,
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken);
    }
}
