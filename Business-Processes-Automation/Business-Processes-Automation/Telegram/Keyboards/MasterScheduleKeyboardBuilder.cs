using Business_Processes_Automation.Telegram.Localization;
using Telegram.Bot.Types.ReplyMarkups;

namespace Business_Processes_Automation.Telegram.Keyboards;

public static class MasterScheduleKeyboardBuilder
{
    public static ReplyKeyboardMarkup BuildViewMenu() =>
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

    public static bool IsMyScheduleEntryButton(string text) =>
        text == TelegramBotTexts.MasterPanel.ButtonMySchedule;

    public static bool IsViewPeriodButton(string text) =>
        text is TelegramBotTexts.MasterSchedule.ButtonViewTomorrow
            or TelegramBotTexts.MasterSchedule.ButtonViewThreeDays
            or TelegramBotTexts.MasterSchedule.ButtonViewWeek
            or TelegramBotTexts.MasterSchedule.ButtonViewMonth;
}
