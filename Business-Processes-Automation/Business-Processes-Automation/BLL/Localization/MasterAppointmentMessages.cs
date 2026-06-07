namespace Business_Processes_Automation.BLL.Localization;

public static class MasterAppointmentMessages
{
    public const string AppointmentNotFound = "Запис не знайдено.";

    public const string ServiceNotFound = "Послугу не знайдено.";

    public const string InvalidDateRange = "Дата «від» має бути не пізніше за дату «до».";

    public const string SlotUnavailable = "Обраний час недоступний для запису.";

    public const string SlotAlreadyTaken = "Цей час уже зайнятий.";

    public const string MinBookingNoticeViolation =
        "Запис можливий лише після мінімального часу до початку послуги.";

    public const string MaxBookingDaysExceeded =
        "Запис можливий лише в межах дозволеного горизонту бронювання.";

    public const string OutsideWorkingHours = "Обраний час поза робочими годинами.";

    public const string NonWorkingDay = "Обраний день не є робочим.";

    public const string FullDayOff = "Обраний день повністю заблоковано.";

    public const string CannotModifyTerminalStatus =
        "Запис у поточному статусі не можна змінити.";

    public const string InvalidStatusTransition = "Недопустима зміна статусу запису.";

    public const string RescheduleLimitReached = "Досягнуто ліміт переносів для цього запису.";

    public const string RescheduleNotAllowed = "Перенос для цього запису недоступний.";

    public const string CancelNotAllowed = "Скасування для цього запису недоступне.";

    public const string NothingToUpdate = "Не вказано полів для оновлення.";

    public const string SettingsNotFound = ScheduleSettingsMessages.AppointmentSettingsNotFound;
}
