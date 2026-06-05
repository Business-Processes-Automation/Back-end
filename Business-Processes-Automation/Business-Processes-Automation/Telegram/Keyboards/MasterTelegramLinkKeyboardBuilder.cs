using Business_Processes_Automation.Telegram.Localization;
using Telegram.Bot.Types.ReplyMarkups;

namespace Business_Processes_Automation.Telegram.Keyboards;

public static class MasterTelegramLinkKeyboardBuilder
{
    public const string ButtonCancel = "Скасувати прив'язку";

    public static ReplyKeyboardMarkup BuildCancel() =>
        new([[new KeyboardButton(ButtonCancel)]])
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = false
        };

    public static bool IsCancel(string text) =>
        text == ButtonCancel;
}
