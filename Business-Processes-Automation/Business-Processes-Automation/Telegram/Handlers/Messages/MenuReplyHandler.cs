using Business_Processes_Automation.BLL.Services;
using Business_Processes_Automation.Telegram.Localization;
using Telegram.Bot;

namespace Business_Processes_Automation.Telegram.Handlers.Messages;

public class MenuReplyHandler(ITelegramUserSessionService sessionService)
{
    public async Task<bool> TryHandleAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        string text,
        CancellationToken cancellationToken = default)
    {
        var session = await sessionService.GetAsync(telegramUserId, cancellationToken);

        if (session?.MasterId is null)
        {
            return false;
        }

        var response = text switch
        {
            TelegramBotTexts.Menu.ButtonServices => TelegramBotTexts.Menu.ServicesStub,
            TelegramBotTexts.Menu.ButtonBook => TelegramBotTexts.Menu.BookStub,
            TelegramBotTexts.Menu.ButtonMyAppointments => TelegramBotTexts.Menu.MyAppointmentsStub,
            TelegramBotTexts.Menu.ButtonAboutMaster => TelegramBotTexts.Menu.AboutMasterStub,
            TelegramBotTexts.Menu.ButtonMasterPanel => TelegramBotTexts.Menu.MasterPanelStub,
            _ => null
        };

        if (response is null)
        {
            return false;
        }

        await botClient.SendMessage(chatId, response, cancellationToken: cancellationToken);
        return true;
    }
}
