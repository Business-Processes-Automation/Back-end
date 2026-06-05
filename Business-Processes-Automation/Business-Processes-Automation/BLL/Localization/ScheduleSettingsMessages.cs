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

    public const string TimeOffOverlap =
        "Цей час уже заблоковано іншим вихідним або перервою.";
}
