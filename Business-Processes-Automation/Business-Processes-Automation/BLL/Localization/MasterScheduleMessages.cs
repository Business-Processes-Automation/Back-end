namespace Business_Processes_Automation.BLL.Localization;

public static class MasterScheduleMessages
{
    public const string NoAppointmentsInPeriod = "Записів у цьому періоді немає.";
    public const string EnterAppointmentNumber = "Введіть номер запису, щоб побачити деталі.";
    public const string DayOff = "Вихідний";
    public const string NotWorking = "Не працюю";
    public const string NoAppointmentsOnDay = "Записів немає";
    public const string DefaultPeriodTitle = "Розклад";

    public static string PeriodTitleTomorrow(DateOnly date) =>
        $"Розклад на завтра ({date:dd.MM.yyyy})";

    public static string PeriodTitleThreeDays(DateOnly start, DateOnly end) =>
        $"Розклад на 3 дні ({start:dd.MM} – {end:dd.MM})";

    public static string PeriodTitleWeek(DateOnly start, DateOnly end) =>
        $"Розклад на тиждень ({start:dd.MM} – {end:dd.MM})";

    public static string PeriodTitleMonth(DateOnly start, DateOnly end) =>
        $"Розклад на місяць ({start:dd.MM} – {end:dd.MM})";

    public static string WorkingHoursLine(TimeOnly start, TimeOnly end) =>
        $"Робочий час: {start:HH:mm} – {end:HH:mm}";

    public static string TimeOffBlockLine(TimeOnly start, TimeOnly end) =>
        $"Блок: {start:HH:mm} – {end:HH:mm}";

    public static string AppointmentDetails(
        string serviceName,
        DateOnly date,
        TimeOnly start,
        TimeOnly end,
        string clientName,
        string clientPhone,
        string status,
        decimal price,
        decimal prepayment) =>
        "Деталі запису\n\n" +
        $"Послуга: {serviceName}\n" +
        $"Дата: {date:dd.MM.yyyy}\n" +
        $"Час: {start:HH:mm} – {end:HH:mm}\n" +
        $"Клієнт: {clientName}\n" +
        $"Телефон: {clientPhone}\n" +
        $"Статус: {status}\n" +
        $"Ціна: {price:0.##} грн\n" +
        $"Передоплата: {prepayment:0.##} грн";
}
