using Business_Processes_Automation.DAL.Enums;
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

    public static ReplyKeyboardMarkup BuildScheduleMenu() =>
        new([
            [new KeyboardButton(TelegramBotTexts.MasterSchedule.ButtonWorkingHours)],
            [new KeyboardButton(TelegramBotTexts.MasterSchedule.ButtonBuffer)],
            [new KeyboardButton(TelegramBotTexts.MasterSchedule.ButtonTimeOff)],
            [new KeyboardButton(TelegramBotTexts.Menu.ButtonBackToMenu)]
        ])
        {
            ResizeKeyboard = true
        };

    public static ReplyKeyboardMarkup BuildWeekdayPicker() =>
        new([
            [
                new KeyboardButton(TelegramBotTexts.MasterSchedule.GetDayLabel(Weekday.Monday)),
                new KeyboardButton(TelegramBotTexts.MasterSchedule.GetDayLabel(Weekday.Tuesday)),
                new KeyboardButton(TelegramBotTexts.MasterSchedule.GetDayLabel(Weekday.Wednesday))
            ],
            [
                new KeyboardButton(TelegramBotTexts.MasterSchedule.GetDayLabel(Weekday.Thursday)),
                new KeyboardButton(TelegramBotTexts.MasterSchedule.GetDayLabel(Weekday.Friday))
            ],
            [
                new KeyboardButton(TelegramBotTexts.MasterSchedule.GetDayLabel(Weekday.Saturday)),
                new KeyboardButton(TelegramBotTexts.MasterSchedule.GetDayLabel(Weekday.Sunday))
            ],
            [new KeyboardButton(TelegramBotTexts.Menu.ButtonBackToMenu)]
        ])
        {
            ResizeKeyboard = true
        };

    public static ReplyKeyboardMarkup BuildWorkHoursStartInput() =>
        new([
            [new KeyboardButton(TelegramBotTexts.MasterSchedule.ButtonNotWorking)],
            [new KeyboardButton(TelegramBotTexts.Menu.ButtonBackToMenu)]
        ])
        {
            ResizeKeyboard = true
        };

    public static ReplyKeyboardMarkup BuildTimeOffMenu() =>
        new([
            [new KeyboardButton(TelegramBotTexts.MasterSchedule.ButtonAddTimeOff)],
            [new KeyboardButton(TelegramBotTexts.MasterSchedule.ButtonDeleteTimeOff)],
            [new KeyboardButton(TelegramBotTexts.Menu.ButtonBackToMenu)]
        ])
        {
            ResizeKeyboard = true
        };

    public static ReplyKeyboardMarkup BuildTimeOffDatePicker() =>
        new([
            [
                new KeyboardButton(TelegramBotTexts.MasterSchedule.ButtonToday),
                new KeyboardButton(TelegramBotTexts.MasterSchedule.ButtonTomorrow)
            ],
            [new KeyboardButton(TelegramBotTexts.Menu.ButtonBackToMenu)]
        ])
        {
            ResizeKeyboard = true
        };

    public static ReplyKeyboardMarkup BuildTimeOffModePicker() =>
        new([
            [new KeyboardButton(TelegramBotTexts.MasterSchedule.ButtonFullDay)],
            [new KeyboardButton(TelegramBotTexts.MasterSchedule.ButtonTimeInterval)],
            [new KeyboardButton(TelegramBotTexts.Menu.ButtonBackToMenu)]
        ])
        {
            ResizeKeyboard = true
        };

    public static ReplyKeyboardMarkup BuildPostRegisterOffer() =>
        new([
            [new KeyboardButton(TelegramBotTexts.MasterSchedule.ButtonSetupSchedule)],
            [new KeyboardButton(TelegramBotTexts.MasterSchedule.ButtonSetupLater)]
        ])
        {
            ResizeKeyboard = true
        };

    public static bool IsScheduleMenuButton(string text) =>
        text is TelegramBotTexts.MasterSchedule.ButtonWorkingHours
            or TelegramBotTexts.MasterSchedule.ButtonBuffer
            or TelegramBotTexts.MasterSchedule.ButtonTimeOff;

    public static bool IsMyScheduleEntryButton(string text) =>
        text == TelegramBotTexts.MasterPanel.ButtonMySchedule;

    public static bool IsScheduleSettingsEntryButton(string text) =>
        text == TelegramBotTexts.MasterPanel.ButtonScheduleSettings;

    public static bool IsViewPeriodButton(string text) =>
        text is TelegramBotTexts.MasterSchedule.ButtonViewTomorrow
            or TelegramBotTexts.MasterSchedule.ButtonViewThreeDays
            or TelegramBotTexts.MasterSchedule.ButtonViewWeek
            or TelegramBotTexts.MasterSchedule.ButtonViewMonth;

    public static bool IsPostRegisterSetupButton(string text) =>
        text is TelegramBotTexts.MasterSchedule.ButtonSetupSchedule
            or TelegramBotTexts.MasterSchedule.ButtonSetupLater;
}
