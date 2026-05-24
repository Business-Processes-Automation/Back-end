using Business_Processes_Automation.DAL.Enums;
using Business_Processes_Automation.Telegram.Localization;
using Telegram.Bot.Types.ReplyMarkups;

namespace Business_Processes_Automation.Telegram.Keyboards;

public static class MenuKeyboardBuilder
{
    public static ReplyKeyboardMarkup BuildClientMenu() =>
        new([
            [new KeyboardButton(TelegramBotTexts.Menu.ButtonServices), new KeyboardButton(TelegramBotTexts.Menu.ButtonBook)],
            [new KeyboardButton(TelegramBotTexts.Menu.ButtonMyAppointments), new KeyboardButton(TelegramBotTexts.Menu.ButtonAboutMaster)]
        ])
        {
            ResizeKeyboard = true
        };

    public static ReplyKeyboardMarkup BuildMasterMenu() =>
        new([
            [new KeyboardButton(TelegramBotTexts.Menu.ButtonServices), new KeyboardButton(TelegramBotTexts.Menu.ButtonBook)],
            [new KeyboardButton(TelegramBotTexts.Menu.ButtonMyAppointments), new KeyboardButton(TelegramBotTexts.Menu.ButtonMasterPanel)]
        ])
        {
            ResizeKeyboard = true
        };

    public static ReplyKeyboardMarkup Build(TelegramUserRole role) =>
        role == TelegramUserRole.Master ? BuildMasterMenu() : BuildClientMenu();
}
