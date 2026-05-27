using Business_Processes_Automation.Telegram.Localization;
using Telegram.Bot.Types.ReplyMarkups;

namespace Business_Processes_Automation.Telegram.Keyboards;

public static class MasterPanelKeyboardBuilder
{
    public static ReplyKeyboardMarkup BuildMain() =>
        new([
            [new KeyboardButton(TelegramBotTexts.MasterPanel.ButtonMySchedule)],
            [new KeyboardButton(TelegramBotTexts.MasterPanel.ButtonScheduleSettings)],
            [new KeyboardButton(TelegramBotTexts.MasterPanel.ButtonMyServices)],
            [new KeyboardButton(TelegramBotTexts.MasterPanel.ButtonAddService)],
            [
                new KeyboardButton(TelegramBotTexts.MasterPanel.ButtonLogout),
                new KeyboardButton(TelegramBotTexts.MasterPanel.ButtonDeleteAccount)
            ]
        ])
        {
            ResizeKeyboard = true
        };

    public static ReplyKeyboardMarkup BuildDeleteConfirmation() =>
        new([
            [new KeyboardButton(TelegramBotTexts.MasterPanel.ButtonConfirmDelete)],
            [new KeyboardButton(TelegramBotTexts.MasterPanel.ButtonCancelDelete)]
        ])
        {
            ResizeKeyboard = true
        };

    public static bool IsPanelActionButton(string text) =>
        text is TelegramBotTexts.MasterPanel.ButtonMyServices
            or TelegramBotTexts.MasterPanel.ButtonAddService;

    public static bool IsAccountButton(string text) =>
        text is TelegramBotTexts.MasterPanel.ButtonLogout
            or TelegramBotTexts.MasterPanel.ButtonDeleteAccount;

    public static bool IsConfirmDelete(string text) =>
        text == TelegramBotTexts.MasterPanel.ButtonConfirmDelete;

    public static bool IsCancelDelete(string text) =>
        text == TelegramBotTexts.MasterPanel.ButtonCancelDelete;
}
