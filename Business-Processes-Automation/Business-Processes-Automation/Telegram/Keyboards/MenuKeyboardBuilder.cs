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

    public static ReplyKeyboardMarkup BuildMasterMenu() => MasterPanelKeyboardBuilder.BuildMain();

    public static ReplyKeyboardMarkup Build(TelegramUserRole role) =>
        role == TelegramUserRole.Master ? BuildMasterMenu() : BuildClientMenu();

    public static string GetMenuTitle(TelegramUserRole role) =>
        role == TelegramUserRole.Master
            ? TelegramBotTexts.MasterPanel.Title
            : TelegramBotTexts.Menu.Title;

    public static ReplyKeyboardMarkup BuildWithBackButton() =>
        new([[new KeyboardButton(TelegramBotTexts.Menu.ButtonBackToMenu)]])
        {
            ResizeKeyboard = true
        };

    public static bool IsBackToMenu(string text) =>
        text.Equals(TelegramBotTexts.Menu.ButtonBackToMenu, StringComparison.Ordinal);

    public static bool IsClientMenuButton(string text) =>
        text is TelegramBotTexts.Menu.ButtonServices
            or TelegramBotTexts.Menu.ButtonBook
            or TelegramBotTexts.Menu.ButtonMyAppointments
            or TelegramBotTexts.Menu.ButtonAboutMaster;
}
