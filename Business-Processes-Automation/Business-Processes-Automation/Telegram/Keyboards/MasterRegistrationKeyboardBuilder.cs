using Business_Processes_Automation.Telegram.Localization;
using Telegram.Bot.Types.ReplyMarkups;

namespace Business_Processes_Automation.Telegram.Keyboards;

public static class MasterRegistrationKeyboardBuilder
{
    public static ReplyKeyboardMarkup BuildConfirmation() =>
        new([
            [new KeyboardButton(TelegramBotTexts.MasterRegistration.ButtonConfirm)],
            [new KeyboardButton(TelegramBotTexts.MasterRegistration.ButtonCancel)]
        ])
        {
            ResizeKeyboard = true
        };

    public static bool IsConfirm(string text) =>
        text == TelegramBotTexts.MasterRegistration.ButtonConfirm;

    public static bool IsCancel(string text) =>
        text == TelegramBotTexts.MasterRegistration.ButtonCancel;
}
