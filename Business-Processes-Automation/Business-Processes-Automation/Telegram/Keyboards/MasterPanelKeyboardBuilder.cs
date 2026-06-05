using Business_Processes_Automation.Telegram.Localization;
using Telegram.Bot.Types.ReplyMarkups;

namespace Business_Processes_Automation.Telegram.Keyboards;

public static class MasterPanelKeyboardBuilder
{
    public static ReplyKeyboardMarkup BuildMain() =>
        new([
            [new KeyboardButton(TelegramBotTexts.MasterPanel.ButtonMySchedule)],
            [new KeyboardButton(TelegramBotTexts.MasterPanel.ButtonMyServices)],
            [new KeyboardButton(TelegramBotTexts.MasterPanel.ButtonLogout)]
        ])
        {
            ResizeKeyboard = true
        };

    public static bool IsPanelActionButton(string text) =>
        text is TelegramBotTexts.MasterPanel.ButtonMyServices;

    public static bool IsAccountButton(string text) =>
        text is TelegramBotTexts.MasterPanel.ButtonLogout;
}
