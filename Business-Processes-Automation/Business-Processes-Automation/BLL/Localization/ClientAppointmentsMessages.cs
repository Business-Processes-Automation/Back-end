namespace Business_Processes_Automation.BLL.Localization;

public static class ClientAppointmentsMessages
{
    public const string Title = "Мої записи до майстра";
    public const string Empty = "У вас поки немає записів у цього майстра.";
    public const string UpcomingSection = "Майбутні:";
    public const string PastSection = "Минулі:";
    public const string EmptySection = "—";

    public static string MasterLine(string displayName) => $"Майстер: {displayName}";

    public static string PhoneLine(string phoneNumber) => $"Телефон: {phoneNumber}";

    public static string TimeZoneLine(string timeZoneId) => $"Часовий пояс: {timeZoneId}";
}
