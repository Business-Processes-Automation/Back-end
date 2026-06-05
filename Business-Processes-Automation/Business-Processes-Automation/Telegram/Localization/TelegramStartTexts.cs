using Business_Processes_Automation.BLL.Localization;

namespace Business_Processes_Automation.Telegram.Localization;

public static partial class TelegramBotTexts
{
    public static class Start
    {
        public const string MissingStartParameter =
            "Щоб записатися до майстра, перейдіть за його посиланням.\n\n" +
            "Щоб підключити Telegram до веб-акаунта — згенеруйте код у кабінеті та надішліть /link.";

        public const string MasterNotFound = CommonMessages.MasterNotFound;

        public static string WelcomeToMaster(string masterDisplayName) =>
            $"Ви записуєтесь до майстра: {masterDisplayName}\n\nОберіть дію в меню нижче.";

        public static string WelcomeRegisteredMaster(string masterDisplayName) =>
            $"Вітаємо, {masterDisplayName}!\n\nПанель майстра. Керуйте послугами та записами.";
    }

    public static class MasterTelegramLink
    {
        public const string AlreadyLinked =
            "Ваш Telegram уже прив'язаний до акаунта майстра.";

        public const string Intro =
            "Прив'язка Telegram до веб-акаунта.\n\n" +
            "1. Увійдіть у веб-кабінет і згенеруйте код прив'язки.\n" +
            "2. Надішліть код сюди (8 символів):";

        public const string PromptBotLink =
            "Введіть ідентифікатор для посилання клієнтів (латиниця, цифри, _):\n" +
            "наприклад: anna_nails";

        public const string InvalidBotLink = MasterTelegramLinkMessages.InvalidBotLinkFormat;

        public const string BotLinkTaken = MasterTelegramLinkMessages.BotLinkTaken;

        public const string Cancelled = "Прив'язку скасовано.";

        public const string SessionExpired = "Сесію прив'язки перервано. Надішліть /link знову.";

        public static string Success(string displayName, string clientLink) =>
            $"Telegram прив'язано, {displayName}!\n\n" +
            "Панель майстра активна.\n\n" +
            $"Посилання для клієнтів:\n{clientLink}";
    }
}
