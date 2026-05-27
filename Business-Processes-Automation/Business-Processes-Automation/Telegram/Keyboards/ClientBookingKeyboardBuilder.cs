using Business_Processes_Automation.Telegram.Localization;
using Telegram.Bot.Types.ReplyMarkups;

namespace Business_Processes_Automation.Telegram.Keyboards;

public static class ClientBookingKeyboardBuilder
{
    public static ReplyKeyboardMarkup BuildPeriodMenu() =>
        new([
            [
                new KeyboardButton(TelegramBotTexts.MasterSchedule.ButtonViewTomorrow),
                new KeyboardButton(TelegramBotTexts.MasterSchedule.ButtonViewThreeDays)
            ],
            [
                new KeyboardButton(TelegramBotTexts.MasterSchedule.ButtonViewWeek),
                new KeyboardButton(TelegramBotTexts.MasterSchedule.ButtonViewMonth)
            ],
            [new KeyboardButton(TelegramBotTexts.Menu.ButtonBackToMenu)]
        ])
        {
            ResizeKeyboard = true
        };

    public static ReplyKeyboardMarkup BuildConfirmation() =>
        new([
            [new KeyboardButton(TelegramBotTexts.ClientBooking.ButtonConfirm)],
            [new KeyboardButton(TelegramBotTexts.ClientBooking.ButtonCancel)],
            [new KeyboardButton(TelegramBotTexts.Menu.ButtonBackToMenu)]
        ])
        {
            ResizeKeyboard = true
        };

    public static bool IsViewPeriodButton(string text) =>
        text is TelegramBotTexts.MasterSchedule.ButtonViewTomorrow
            or TelegramBotTexts.MasterSchedule.ButtonViewThreeDays
            or TelegramBotTexts.MasterSchedule.ButtonViewWeek
            or TelegramBotTexts.MasterSchedule.ButtonViewMonth;

    public static bool IsConfirm(string text) =>
        text == TelegramBotTexts.ClientBooking.ButtonConfirm;

    public static bool IsCancel(string text) =>
        text == TelegramBotTexts.ClientBooking.ButtonCancel;
}
