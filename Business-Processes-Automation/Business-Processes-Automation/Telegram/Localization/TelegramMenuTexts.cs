using Business_Processes_Automation.BLL.DTOs.Service;
using Business_Processes_Automation.BLL.Helpers;
using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.Telegram.Localization;

public static partial class TelegramBotTexts
{
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

        public static string FormatServicesList(IReadOnlyList<ServiceResponseDTO> services)
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
}
