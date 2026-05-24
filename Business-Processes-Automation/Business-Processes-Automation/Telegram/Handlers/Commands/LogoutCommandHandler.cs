using Business_Processes_Automation.BLL.Services;
using Business_Processes_Automation.Telegram.Abstractions;
using Business_Processes_Automation.Telegram.Localization;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Business_Processes_Automation.Telegram.Handlers.Commands;

public class LogoutCommandHandler : ITelegramCommandHandler
{
    private readonly IMasterAccountService _accountService;
    private readonly IMasterRegistrationService _registrationService;

    public LogoutCommandHandler(
        IMasterAccountService accountService,
        IMasterRegistrationService registrationService)
    {
        _accountService = accountService;
        _registrationService = registrationService;
    }

    public bool CanHandle(string messageText) =>
        messageText.StartsWith("/logout", StringComparison.OrdinalIgnoreCase);

    public async Task HandleAsync(TelegramCommandContext context, CancellationToken cancellationToken = default)
    {
        if (context.TelegramUserId is not { } telegramUserId)
        {
            return;
        }

        var masterTelegram = await _registrationService.GetByTelegramUserIdAsync(telegramUserId, cancellationToken);
        if (masterTelegram is null)
        {
            await context.BotClient.SendMessage(
                context.ChatId,
                TelegramBotTexts.MasterPanel.DeleteAccountNotFound,
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
