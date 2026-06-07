using Business_Processes_Automation.BLL.Localization;

namespace Business_Processes_Automation.Telegram.Localization;

public static partial class TelegramBotTexts
{
    public static class ClientBooking
    {
        public const string PeriodMenuTitle = "Оберіть період для запису:";

        public const string ButtonConfirm = "Підтвердити запис";
        public const string ButtonCancel = "Скасувати запис";
        public const string ButtonBackToDates = "◀ Дати";

        public const string ChooseService = "Оберіть послугу:";
        public const string ChooseDate = "Оберіть дату для запису:";
        public const string ChooseSlot = ClientBookingMessages.ChooseSlot;

        public const string InvalidService = "Оберіть послугу зі списку.";
        public const string InvalidDate = "Оберіть дату зі списку.";
        public const string InvalidSlot = "Оберіть час зі списку.";
        public const string NoSlotsInPeriod = ClientBookingMessages.NoSlotsInPeriod;
        public const string NoSlotsOnDay = ClientBookingMessages.NoSlotsOnDay;
        public const string NoServices = "У цього майстра поки немає послуг для запису.";
        public const string BookingSuccessAck =
            "Запис оформлено! Деталі запису надійдуть окремим повідомленням.";
        public const string BookingCancelled = "Запис скасовано.";
        public const string UseButtons = "Натисніть кнопку підтвердження або скасування.";
    }
}
