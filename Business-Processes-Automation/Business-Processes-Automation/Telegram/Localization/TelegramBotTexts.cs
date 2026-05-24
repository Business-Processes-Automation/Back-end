namespace Business_Processes_Automation.Telegram.Localization;

/// <summary>
/// User-facing bot messages (Ukrainian).
/// </summary>
public static class TelegramBotTexts
{
    public static class Common
    {
        public const string UnexpectedError =
            "Сталася помилка. Спробуйте пізніше або надішліть /start.";

        public const string SessionRequired =
            "Спочатку перейдіть за посиланням вашого майстра або надішліть /start з параметром.";
    }

    public static class Start
    {
        public const string MissingStartParameter =
            "Щоб записатися, перейдіть за посиланням від вашого майстра.";

        public const string MasterNotFound =
            "Майстра не знайдено. Перевірте посилання або зверніться до майстра.";

        public static string WelcomeToMaster(string masterDisplayName) =>
            $"Ви записуєтесь до майстра: {masterDisplayName}\n\nОберіть дію в меню нижче.";
    }

    public static class Menu
    {
        public const string Title = "Головне меню:";

        public const string CancelConfirmed =
            "Дію скасовано. Оберіть пункт меню нижче.";

        public const string ButtonServices = "Послуги";
        public const string ButtonBook = "Записатися";
        public const string ButtonMyAppointments = "Мої записи";
        public const string ButtonAboutMaster = "Про майстра";
        public const string ButtonMasterPanel = "Панель майстра";

        public const string ServicesStub = "Послуги (демо). Незабаром тут буде список послуг майстра.";
        public const string BookStub = "Запис (демо). Незабаром тут буде вибір дати та часу.";
        public const string MyAppointmentsStub = "У вас поки немає записів.";
        public const string AboutMasterStub = "Інформація про майстра (демо).";
        public const string MasterPanelStub = "Панель майстра (демо).";
    }
}
