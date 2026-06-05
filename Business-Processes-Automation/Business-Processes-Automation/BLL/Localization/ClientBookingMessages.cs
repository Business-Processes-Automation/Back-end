namespace Business_Processes_Automation.BLL.Localization;

public static class ClientBookingMessages
{
    public const string NoSlotsOnDay = "На цей день немає вільних слотів.";
    public const string ChooseSlot = "Оберіть час для запису:";
    public const string ServicesForBookingHeader = "Послуги для запису:";
    public const string EnterServiceNumber = "Введіть номер послуги, щоб побачити вільні слоти.";
    public const string NoFreeSlotsLabel = "Вільних слотів немає";
    public const string FreeSlotsHeader = "Вільні слоти:";
    public const string NoSlotsInPeriod = "Немає вільних слотів для цієї послуги в обраному періоді.";
    public const string EnterSlotNumber = "Введіть номер слота для запису.";
    public const string DayOff = "Вихідний";
    public const string MasterNotWorking = "Майстер не працює";
    public const string SlotUnavailable = "Цей час більше недоступний.";
    public const string SlotAlreadyTaken = "Цей час вже зайнятий.";
    public const string DefaultPeriodTitle = "Запис";

    public static string WorkingHoursLine(TimeOnly start, TimeOnly end) =>
        $"Робочий час: {start:HH:mm} – {end:HH:mm}";

    public static string UnavailableBlock(DateTime start, DateTime end) =>
        $"Недоступно: {start:HH:mm} – {end:HH:mm}";

    public static string OccupiedBlock(DateTime start, DateTime end) =>
        $"Зайнято: {start:HH:mm}–{end:HH:mm}";

    public static string PeriodTitleTomorrow(DateOnly date) =>
        $"Запис на завтра ({date:dd.MM.yyyy})";

    public static string PeriodTitleThreeDays(DateOnly start, DateOnly end) =>
        $"Запис на 3 дні ({start:dd.MM} – {end:dd.MM})";

    public static string PeriodTitleWeek(DateOnly start, DateOnly end) =>
        $"Запис на тиждень ({start:dd.MM} – {end:dd.MM})";

    public static string PeriodTitleMonth(DateOnly start, DateOnly end) =>
        $"Запис на місяць ({start:dd.MM} – {end:dd.MM})";

    public static string DayServiceHeader(DateOnly date, string serviceName) =>
        $"{date:dd.MM.yyyy} — {serviceName}";

    public static string ServiceLine(string serviceName, int durationMinutes) =>
        $"Послуга: {serviceName} ({durationMinutes} хв)";

    public static string ConfirmationHeader(string serviceName, DateOnly date, TimeOnly start, DateTime endLocal, int durationMinutes, decimal price, decimal prepayment) =>
        "Підтвердіть запис:\n\n" +
        $"Послуга: {serviceName}\n" +
        $"Дата: {date:dd.MM.yyyy}\n" +
        $"Час: {start:HH:mm} – {endLocal:HH:mm}\n" +
        $"Тривалість: {durationMinutes} хв\n" +
        $"Ціна: {price:0.##} грн\n" +
        $"Передоплата: {prepayment:0.##} грн";
}
