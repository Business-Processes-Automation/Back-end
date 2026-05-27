using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.Telegram.Helpers;

public static class MasterScheduleSteps
{
    public static bool IsScheduleStep(ConversationStep step) =>
        step is ConversationStep.MasterScheduleMenu
            or ConversationStep.MasterEditingWorkHoursPickDay
            or ConversationStep.MasterEditingWorkHoursStart
            or ConversationStep.MasterEditingWorkHoursEnd
            or ConversationStep.MasterEditingBuffer
            or ConversationStep.MasterAddingTimeOffMenu
            or ConversationStep.MasterAddingTimeOffPickDate
            or ConversationStep.MasterAddingTimeOffPickMode
            or ConversationStep.MasterAddingTimeOffStartTime
            or ConversationStep.MasterAddingTimeOffEndTime
            or ConversationStep.MasterDeletingTimeOffPickNumber
            or ConversationStep.MasterPostRegisterScheduleOffer;

    public static bool IsScheduleViewStep(ConversationStep step) =>
        step is ConversationStep.MasterScheduleViewMenu
            or ConversationStep.MasterViewingAppointmentPickNumber;
}
