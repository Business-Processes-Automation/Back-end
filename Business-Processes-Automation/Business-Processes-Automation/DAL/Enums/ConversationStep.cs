namespace Business_Processes_Automation.DAL.Enums;

public enum ConversationStep
{
    Idle = 0,
    ChoosingService = 1,
    ChoosingDate = 2,
    ChoosingTime = 3,
    ConfirmingBooking = 4,
    MasterPanelMenu = 10,
    MasterAddingServiceName = 11,
    MasterAddingServiceDuration = 12,
    MasterAddingServicePrice = 13,
    MasterRegisteringFirstName = 20,
    MasterRegisteringLastName = 21,
    MasterRegisteringPhone = 22,
    MasterRegisteringEmail = 23,
    MasterRegisteringTimeZone = 24,
    MasterRegisteringBotLink = 25,
    MasterRegisteringConfirm = 26,
    MasterConfirmingDeleteAccount = 27
}
