using Business_Processes_Automation.BLL.DTOs.Service;
using Business_Processes_Automation.BLL.Helpers;
using Business_Processes_Automation.BLL.Localization;

namespace Business_Processes_Automation.Telegram.Localization;

public static partial class TelegramBotTexts
{
    public static class MasterPanel
    {
        public const string Title = "Панель майстра. Оберіть дію:";

        public const string ButtonMySchedule = "Мій розклад";
        public const string ButtonMyServices = "Мої послуги";
        public const string ButtonLogout = "Вийти з акаунту";

        public const string LoggedOut =
            "Ви вийшли з акаунту. Щоб знову увійти — /start з вашим посиланням або /link для прив'язки.";

        public const string MasterAccountNotFound = AuthMessages.MasterAccountNotFound;

        public const string ClientsOnly =
            "Цей розділ доступний лише майстру.";

        public static string FormatServicesList(IReadOnlyList<ServiceResponseDTO> services)
        {
            if (services.Count == 0)
            {
                return "У вас поки немає послуг. Додайте їх у веб-кабінеті.";
            }

            var lines = services.Select((service, index) =>
                ScheduleDisplayHelper.FormatServiceLine(service, index + 1));

            return "Ваші послуги:\n\n" + string.Join('\n', lines);
        }
    }
}
