using Business_Processes_Automation.BLL.Helpers;
using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.Telegram.Localization;

public static partial class TelegramBotTexts
{
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
