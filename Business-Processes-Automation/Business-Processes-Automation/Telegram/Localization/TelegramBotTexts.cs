using Business_Processes_Automation.BLL.Helpers;
using Business_Processes_Automation.BLL.Localization;
using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;

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

        public const string MasterNotFound = UserMessages.MasterNotFound;

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

        public static string AboutMaster(Master master, string displayName) =>
            "Про майстра\n\n" +
            $"Ім'я: {displayName}\n" +
            $"Телефон: {master.PhoneNumber}\n" +
            $"Часовий пояс: {master.TimeZone}";

        public static string FormatServicesList(IReadOnlyList<Service> services)
        {
            if (services.Count == 0)
            {
                return "У цього майстра поки немає послуг.";
            }

            var lines = services.Select((service, index) =>
                ScheduleDisplayHelper.FormatServiceLine(service, index + 1));

            return string.Join('\n', lines);
        }
    }

    public static class MasterPanel
    {
        public const string Title = "Панель майстра. Оберіть дію:";

        public const string ButtonMySchedule = "Мій розклад";
        public const string ButtonScheduleSettings = "Налаштування розкладу";
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

            var lines = services.Select((service, index) =>
                ScheduleDisplayHelper.FormatServiceLine(service, index + 1));

            return "Ваші послуги:\n\n" + string.Join('\n', lines);
        }
    }

    public static class ClientBooking
    {
        public const string PeriodMenuTitle = "Оберіть період для запису:";

        public const string ButtonConfirm = "Підтвердити запис";
        public const string ButtonCancel = "Скасувати запис";
        public const string ButtonBackToDates = "◀ Дати";

        public const string ChooseService = "Оберіть послугу:";
        public const string ChooseDate = "Оберіть дату для запису:";
        public const string ChooseSlot = "Оберіть час для запису:";

        public const string InvalidService = "Оберіть послугу зі списку.";
        public const string InvalidDate = "Оберіть дату зі списку.";
        public const string InvalidSlot = "Оберіть час зі списку.";
        public const string NoSlotsInPeriod = "Немає вільних слотів для цієї послуги в обраному періоді.";
        public const string NoSlotsOnDay = "На цей день немає вільних слотів.";
        public const string NoServices = "У цього майстра поки немає послуг для запису.";
        public const string BookingSuccess = "Запис підтверджено! Очікуємо вас у зазначений час.";
        public const string BookingCancelled = "Запис скасовано.";
        public const string UseButtons = "Натисніть кнопку підтвердження або скасування.";
    }

    public static class MasterSchedule
    {
        public const string ViewMenuTitle = "Мій розклад. Оберіть період:";

        public const string ButtonViewTomorrow = "Завтра";
        public const string ButtonViewThreeDays = "3 дні";
        public const string ButtonViewWeek = "Тиждень";
        public const string ButtonViewMonth = "Місяць";

        public const string AppointmentNotFound = "Запис не знайдено.";
        public const string InvalidAppointmentNumber = "Введіть номер зі списку (наприклад, 1).";

        public const string MenuTitle = "Налаштування розкладу. Оберіть дію:";

        public const string ButtonWorkingHours = "Робочі години";
        public const string ButtonBuffer = "Перерва між записами";
        public const string ButtonSlotInterval = "Крок вільних слотів";
        public const string ButtonSlotInterval5 = "5 хв";
        public const string ButtonSlotInterval10 = "10 хв";
        public const string ButtonSlotInterval15 = "15 хв";
        public const string ButtonSlotInterval20 = "20 хв";
        public const string ButtonSlotInterval30 = "30 хв";
        public const string ButtonSlotInterval60 = "60 хв";
        public const string ButtonTimeOff = "Вихідні / блоки";
        public const string ButtonAddTimeOff = "Додати вихідний";
        public const string ButtonDeleteTimeOff = "Видалити вихідний";
        public const string ButtonNotWorking = "Не працюю";
        public const string ButtonFullDay = "Цілий день";
        public const string ButtonTimeInterval = "Інтервал часу";
        public const string ButtonToday = "Сьогодні";
        public const string ButtonTomorrow = "Завтра";
        public const string ButtonSetupSchedule = "Налаштувати робочі години";
        public const string ButtonSetupLater = "Пізніше";

        public const string WorkingHoursTitle = "Робочі години. Оберіть день для зміни:";
        public const string TimeOffTitle = "Вихідні та блоки часу:";
        public const string TimeOffEmpty = "Немає запланованих вихідних на найближчі 14 днів.";

        public const string PostRegisterOffer =
            "Бажаєте налаштувати робочі години зараз?\n\n" +
            "Решту днів можна змінити пізніше в «Налаштування розкладу».";

        public const string PostRegisterOfferPrompt =
            "Оберіть «Налаштувати робочі години» або «Пізніше».";

        public const string PickDayFromButtons = "Оберіть день з кнопок нижче.";

        public const string PostRegisterHint =
            "Оберіть дні та години. Сб–Нд можна залишити вихідними.";

        public const string InvalidTime =
            "Введіть час у форматі ГГ:ХХ (наприклад, 10:00).";

        public const string InvalidDate =
            "Введіть дату у форматі ДД.ММ або ДД.ММ.РРРР, або оберіть кнопку нижче.";

        public const string InvalidBuffer =
            "Введіть ціле число хвилин від 0 до 480.";

        public const string InvalidSlotInterval =
            "Оберіть кнопку або введіть хвилини від 5 до 120 (кратно 5: 5, 10, 15, 30…).";

        public const string InvalidTimeOffNumber =
            "Введіть номер зі списку (наприклад, 1).";

        public const string WorkingHoursSaved = "Робочі години збережено.";
        public const string DayMarkedNotWorking = "День позначено як вихідний.";
        public const string BufferSaved = "Перерву між записами збережено.";
        public const string SlotIntervalSaved = "Крок вільних слотів збережено.";
        public const string TimeOffCreated = "Вихідний / блок часу додано.";
        public const string TimeOffDeleted = "Запис видалено.";
        public const string TimeOffDeleteEmpty = "Немає записів для видалення.";

        public static string PromptWorkStart(string dayLabel) =>
            $"{dayLabel}. Введіть час початку (наприклад, 10:00) або натисніть «Не працюю»:";

        public static string PromptWorkEnd(string dayLabel) =>
            $"{dayLabel}. Введіть час закінчення (наприклад, 19:00):";

        public static string PromptBuffer(int currentMinutes) =>
            $"Зараз перерва між записами: {currentMinutes} хв.\n\nВведіть нове значення (0–480):";

        public static string PromptSlotInterval(int currentMinutes) =>
            $"Зараз клієнтам пропонуються слоти кожні {currentMinutes} хв.\n\n" +
            "Оберіть кнопку або введіть хвилини (5–120, кратно 5).\n\n" +
            "Менший крок — більше варіантів часу; більший — менше кнопок у чаті.";

        public const string PromptTimeOffDate =
            "Оберіть дату вихідного або введіть у форматі ДД.ММ:";

        public const string PromptTimeOffMode =
            "Оберіть тип блокування:";

        public const string PromptTimeOffStart =
            "Введіть час початку блоку (наприклад, 13:00):";

        public const string PromptTimeOffEnd =
            "Введіть час закінчення блоку (наприклад, 14:00):";

        public const string PromptTimeOffDeleteNumber =
            "Введіть номер запису для видалення:";

        public static string GetDayLabel(Weekday day) => ScheduleDisplayHelper.GetDayLabel(day);

        public static bool TryParseDayLabel(string text, out Weekday day) =>
            ScheduleDisplayHelper.TryParseDayLabel(text, out day);

        public static string FormatWorkingHoursList(
            IReadOnlyList<WorkingHoursPerDay> workingHours)
        {
            var byDay = workingHours.ToDictionary(x => x.DayOfWeek);

            var lines = Enum.GetValues<Weekday>()
                .OrderBy(d => (int)d)
                .Select(day =>
                {
                    if (byDay.TryGetValue(day, out var entry))
                    {
                        return $"{GetDayLabel(day)}: {entry.WorkStartTime:HH:mm} – {entry.WorkEndTime:HH:mm}";
                    }

                    return $"{GetDayLabel(day)}: не працюю";
                });

            return "Робочі години:\n\n" + string.Join('\n', lines);
        }

        public static string FormatTimeOffList(
            IReadOnlyList<TimeOff> timeOffs,
            TimeZoneInfo timeZone)
        {
            if (timeOffs.Count == 0)
            {
                return TimeOffEmpty;
            }

            var lines = timeOffs.Select((entry, index) =>
            {
                var start = TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.SpecifyKind(entry.StartDateTime, DateTimeKind.Utc),
                    timeZone);
                var end = TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.SpecifyKind(entry.EndDateTime, DateTimeKind.Utc),
                    timeZone);

                var startDate = DateOnly.FromDateTime(start);
                var endDate = DateOnly.FromDateTime(end);
                var isFullDay = start.TimeOfDay == TimeSpan.Zero
                    && end.TimeOfDay >= TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59));

                if (startDate == endDate && isFullDay)
                {
                    return $"{index + 1}) {startDate:dd.MM} — весь день";
                }

                if (startDate == endDate)
                {
                    return $"{index + 1}) {startDate:dd.MM} — {start:HH:mm}–{end:HH:mm}";
                }

                return $"{index + 1}) {start:dd.MM HH:mm} – {end:dd.MM HH:mm}";
            });

            return TimeOffTitle + "\n\n" + string.Join('\n', lines);
        }
    }
}

