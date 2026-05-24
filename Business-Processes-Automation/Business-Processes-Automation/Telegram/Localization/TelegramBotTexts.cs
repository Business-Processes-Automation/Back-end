using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.Telegram.Localization;

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
            "Щоб записатися до майстра, перейдіть за його посиланням.\n\n" +
            "Щоб стати майстром — надішліть /register";

        public const string MasterNotFound =
            "Майстра не знайдено. Перевірте посилання або зверніться до майстра.";

        public static string WelcomeToMaster(string masterDisplayName) =>
            $"Ви записуєтесь до майстра: {masterDisplayName}\n\nОберіть дію в меню нижче.";

        public static string WelcomeRegisteredMaster(string masterDisplayName) =>
            $"Вітаємо, {masterDisplayName}!\n\nПанель майстра. Керуйте послугами та записами.";
    }

    public static class MasterRegistration
    {
        public const string AlreadyRegistered =
            "Ви вже зареєстровані як майстер.";

        public const string Intro =
            "Реєстрація майстра. Заповніть дані по кроках.\n\nВведіть ваше ім'я:";

        public const string PromptLastName = "Введіть прізвище:";
        public const string PromptPhone = "Введіть номер телефону (наприклад, +380501234567):";
        public const string PromptEmail = "Введіть email:";
        public const string PromptTimeZone =
            "Введіть часовий пояс (наприклад, Europe/Kyiv) або надішліть «-» для Europe/Kyiv:";

        public const string PromptBotLink =
            "Введіть ідентифікатор для посилання клієнтів (латиниця, цифри, _):\n" +
            "наприклад: anna_nails";

        public const string ButtonConfirm = "Підтвердити реєстрацію";
        public const string ButtonCancel = "Скасувати реєстрацію";

        public const string InvalidFirstName = "Ім'я має містити від 1 до 100 символів.";
        public const string InvalidLastName = "Прізвище має містити від 1 до 100 символів.";
        public const string InvalidPhone = "Телефон має містити від 5 до 20 символів.";
        public const string InvalidEmail = "Введіть коректний email.";
        public const string InvalidBotLink =
            "Ідентифікатор: латинські літери, цифри та _, від 3 до 32 символів (наприклад, my_salon).";

        public const string BotLinkTaken = "Цей ідентифікатор вже зайнятий. Введіть інший.";
        public const string Cancelled = "Реєстрацію скасовано.";

        public static string ConfirmationSummary(MasterRegistrationSummary summary) =>
            "Перевірте дані:\n\n" +
            $"Ім'я: {summary.FirstName} {summary.LastName}\n" +
            $"Телефон: {summary.PhoneNumber}\n" +
            $"Email: {summary.Email}\n" +
            $"Часовий пояс: {summary.TimeZone}\n" +
            $"Посилання: {summary.BotLinkLabel}\n\n" +
            "Натисніть «Підтвердити реєстрацію» або «Скасувати реєстрацію».";

        public static string Success(string displayName, string clientLink) =>
            $"Реєстрацію завершено, {displayName}!\n\n" +
            "Ваше меню майстра активне.\n\n" +
            $"Посилання для клієнтів:\n{clientLink}";
    }

    public sealed class MasterRegistrationSummary
    {
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string TimeZone { get; init; } = string.Empty;
        public string BotLinkLabel { get; init; } = string.Empty;
    }

    public static class Menu
    {
        public const string Title = "Головне меню:";

        public const string CancelConfirmed =
            "Дію скасовано. Оберіть пункт меню нижче.";

        public const string CancelConfirmedMaster =
            "Дію скасовано. Оберіть дію в панелі майстра.";

        public const string ButtonServices = "Послуги";
        public const string ButtonBook = "Записатися";
        public const string ButtonMyAppointments = "Мої записи";
        public const string ButtonAboutMaster = "Про майстра";
        public const string ButtonMasterPanel = "Панель майстра";
        public const string ButtonBackToMenu = "Назад в меню";

        public const string MyAppointmentsStub = "У вас поки немає записів.";

        public static string AboutMaster(Master master, string displayName) =>
            "Про майстра\n\n" +
            $"Ім'я: {displayName}\n" +
            $"Телефон: {master.PhoneNumber}\n" +
            $"Часовий пояс: {master.TimeZone}";

        public static string ChoosingServiceIntro() =>
            "Оберіть послугу (демо). Незабаром тут буде вибір дати та часу.\n\n";

        public static string FormatServicesList(IReadOnlyList<Service> services)
        {
            if (services.Count == 0)
            {
                return "У цього майстра поки немає послуг.";
            }

            var lines = services.Select(static (service, index) =>
                $"{index + 1}. {service.ServiceName} — {service.DurationInMinutes} хв, {service.Price:0} грн");

            return ChoosingServiceIntro() + string.Join('\n', lines);
        }
    }

    public static class MasterPanel
    {
        public const string Title = "Панель майстра. Оберіть дію:";

        public const string ButtonMyServices = "Мої послуги";
        public const string ButtonAddService = "Додати послугу";
        public const string ButtonLogout = "Вийти з акаунту";
        public const string ButtonDeleteAccount = "Видалити акаунт";

        public const string ButtonConfirmDelete = "Так, видалити акаунт";
        public const string ButtonCancelDelete = "Ні, залишити";

        public const string LoggedOut =
            "Ви вийшли з акаунту. Щоб знову увійти — /start з вашим посиланням або /register.";

        public const string DeleteAccountWarning =
            "Видалення акаунту безповоротно приховає ваш профіль, послуги та посилання для клієнтів.\n\n" +
            "Підтвердіть дію:";

        public const string DeleteAccountSuccess =
            "Акаунт видалено. Щоб знову стати майстром — надішліть /register.";

        public const string DeleteAccountCancelled = "Видалення скасовано.";

        public const string DeleteAccountNotFound =
            "Обліковий запис майстра не знайдено.";

        public const string ClientsOnly =
            "Цей розділ доступний лише майстру.";

        public const string PromptServiceName =
            "Введіть назву послуги (наприклад, «Манікюр»):";

        public const string PromptServiceDuration =
            "Введіть тривалість у хвилинах (наприклад, 60):";

        public const string PromptServicePrice =
            "Введіть ціну в грн (наприклад, 500):";

        public const string InvalidServiceName =
            "Назва має містити від 2 до 200 символів. Спробуйте ще раз.";

        public const string InvalidDuration =
            "Введіть ціле число хвилин від 1 до 1440.";

        public const string InvalidPrice =
            "Введіть ціну числом від 0 до 999999.99 (наприклад, 500 або 500.50).";

        public static string ServiceCreated(string serviceName, int durationMinutes, decimal price) =>
            "Послугу додано!\n\n" +
            $"Назва: {serviceName}\n" +
            $"Тривалість: {durationMinutes} хв\n" +
            $"Ціна: {price:0.##} грн";

        public static string FormatServicesList(IReadOnlyList<Service> services)
        {
            if (services.Count == 0)
            {
                return "У вас поки немає послуг.\n\nНатисніть «Додати послугу», щоб створити першу.";
            }

            var lines = services.Select(static (service, index) =>
                $"{index + 1}. {service.ServiceName} — {service.DurationInMinutes} хв, {service.Price:0} грн");

            return "Ваші послуги:\n\n" + string.Join('\n', lines);
        }
    }
}
