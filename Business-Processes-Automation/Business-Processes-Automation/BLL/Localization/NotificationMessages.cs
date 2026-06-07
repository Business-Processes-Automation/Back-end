namespace Business_Processes_Automation.BLL.Localization;

public static class NotificationMessages
{
    public static string BookingConfirmedTitle => "✅ Запис підтверджено!";

    public static string Reminder24HoursTitle => "⏰ Нагадування про запис";

    public static string Reminder1HourTitle => "⏰ Нагадування: через годину";

    public static string BookingCancelledTitle => "❌ Запис скасовано.";

    public static string BookingRescheduledTitle => "🔄 Запис перенесено.";

    public static string FormatServiceAppointment(
        string serviceName,
        DateOnly date,
        TimeOnly start,
        DateTime endLocal,
        int durationMinutes) =>
        $"Послуга: {serviceName}\n" +
        $"Дата: {date:dd.MM.yyyy}\n" +
        $"Час: {start:HH:mm} – {endLocal:HH:mm}\n" +
        $"Тривалість: {durationMinutes} хв";

    public static string FormatMasterLine(string masterDisplayName, string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return $"Майстер: {masterDisplayName}";
        }

        return $"Майстер: {masterDisplayName}\nТелефон: {phoneNumber.Trim()}";
    }

    public static string FormatReminder24HoursWhen(DateOnly appointmentDate, TimeOnly start, bool isTomorrow) =>
        isTomorrow
            ? $"Завтра, {appointmentDate:dd.MM.yyyy} о {start:HH:mm}"
            : $"{appointmentDate:dd.MM.yyyy} о {start:HH:mm}";

    public static string FormatReminder1HourWhen(DateOnly appointmentDate, TimeOnly start) =>
        $"Сьогодні о {start:HH:mm} ({appointmentDate:dd.MM.yyyy})";

    public static string FormatPreviousSlot(
        DateOnly date,
        TimeOnly start,
        DateTime endLocal) =>
        $"Було: {date:dd.MM.yyyy}, {start:HH:mm} – {endLocal:HH:mm}";

    public static string FormatNewSlot(
        DateOnly date,
        TimeOnly start,
        DateTime endLocal) =>
        $"Стало: {date:dd.MM.yyyy}, {start:HH:mm} – {endLocal:HH:mm}";
}
