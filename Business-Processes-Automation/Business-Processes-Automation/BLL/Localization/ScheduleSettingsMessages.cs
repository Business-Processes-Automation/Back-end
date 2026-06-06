namespace Business_Processes_Automation.BLL.Localization;

public static class ScheduleSettingsMessages
{
    public const string EndTimeMustBeAfterStart =
        "Час закінчення має бути пізніше за час початку.";

    public const string BufferOutOfRange =
        "Перерва має бути від 0 до 480 хвилин.";

    public const string AppointmentSettingsNotFound =
        "Налаштування записів не знайдено.";

    public const string SlotIntervalOutOfRange =
        "Крок слотів: від 5 до 120 хв, кратно 5 (наприклад, 15 або 30).";

    public const string MinBookingNoticeOutOfRange =
        "Мінімальний час до запису: від 0 до 10080 хвилин.";

    public const string WorkingHoursMustContainSevenDays =
        "Потрібно передати рівно 7 днів тижня.";

    public const string WorkingHoursDuplicateDay =
        "Кожен день тижня має бути вказаний лише один раз.";

    public const string WorkingHoursInvalidDay =
        "Невірний день тижня.";

    public const string WorkingDayTimesRequired =
        "Для робочого дня вкажіть час початку та закінчення.";

    public const string InvalidWorkTimeFormat =
        "Час має бути у форматі ГГ:ХХ (наприклад, 10:00).";

    public const string WorkingDayTooShort =
        "Робочий день має тривати щонайменше 30 хвилин.";

    public const string TimeOffOverlap =
        "Цей час уже заблоковано іншим вихідним або перервою.";

    public const string TimeOffNotFound =
        "Вихідний або блок часу не знайдено.";

    public const string TimeOffDateRangeInvalid =
        "Дата «від» має бути не пізніше за дату «до».";
}
