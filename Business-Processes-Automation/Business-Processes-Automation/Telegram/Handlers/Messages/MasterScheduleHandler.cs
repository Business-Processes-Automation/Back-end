using Business_Processes_Automation.BLL.Enums;
using Business_Processes_Automation.BLL.Helpers;
using Business_Processes_Automation.BLL.Services;
using Business_Processes_Automation.BLL.SessionDrafts;
using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;
using Business_Processes_Automation.Telegram.Helpers;
using Business_Processes_Automation.Telegram.Keyboards;
using Business_Processes_Automation.Telegram.Localization;
using Telegram.Bot;

namespace Business_Processes_Automation.Telegram.Handlers.Messages;

public class MasterScheduleHandler
{
    private const int TimeOffListDaysAhead = 14;

    private readonly ITelegramUserSessionService _sessionService;
    private readonly IMasterScheduleSettingsService _scheduleSettingsService;
    private readonly IMasterAvailabilityService _availabilityService;
    private readonly IMasterScheduleViewService _scheduleViewService;

    public MasterScheduleHandler(
        ITelegramUserSessionService sessionService,
        IMasterScheduleSettingsService scheduleSettingsService,
        IMasterAvailabilityService availabilityService,
        IMasterScheduleViewService scheduleViewService)
    {
        _sessionService = sessionService;
        _scheduleSettingsService = scheduleSettingsService;
        _availabilityService = availabilityService;
        _scheduleViewService = scheduleViewService;
    }

    public async Task<bool> TryHandleAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        string text,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionService.GetAsync(telegramUserId, cancellationToken);

        if (session?.MasterId is not { } masterId)
        {
            return false;
        }

        if (session.Role != TelegramUserRole.Master)
        {
            if (IsClientBookingStep(session.CurrentStep))
            {
                return false;
            }

            if (IsMasterOnlyScheduleButton(text))
            {
                await botClient.SendMessage(
                    chatId,
                    TelegramBotTexts.MasterPanel.ClientsOnly,
                    replyMarkup: MenuKeyboardBuilder.Build(session.Role),
                    cancellationToken: cancellationToken);
                return true;
            }

            return false;
        }

        if (MenuKeyboardBuilder.IsBackToMenu(text))
        {
            return false;
        }

        if (session.CurrentStep == ConversationStep.MasterPostRegisterScheduleOffer)
        {
            await HandlePostRegisterOfferAsync(botClient, chatId, telegramUserId, text, cancellationToken);
            return true;
        }

        if (session.CurrentStep == ConversationStep.MasterViewingAppointmentPickNumber)
        {
            await HandleAppointmentPickNumberAsync(
                botClient,
                chatId,
                telegramUserId,
                masterId,
                session,
                text,
                cancellationToken);
            return true;
        }

        if (session.CurrentStep == ConversationStep.MasterScheduleViewMenu)
        {
            await HandleScheduleViewMenuAsync(
                botClient,
                chatId,
                telegramUserId,
                masterId,
                text,
                cancellationToken);
            return true;
        }

        if (MasterScheduleSteps.IsScheduleStep(session.CurrentStep))
        {
            await HandleScheduleStepAsync(
                botClient,
                chatId,
                telegramUserId,
                masterId,
                session,
                text,
                cancellationToken);
            return true;
        }

        if (IsAtMasterMainMenu(session.CurrentStep) &&
            MasterScheduleKeyboardBuilder.IsMyScheduleEntryButton(text))
        {
            await ShowScheduleViewMenuAsync(botClient, chatId, telegramUserId, cancellationToken);
            return true;
        }

        if (IsAtMasterMainMenu(session.CurrentStep) &&
            MasterScheduleKeyboardBuilder.IsScheduleSettingsEntryButton(text))
        {
            await ShowScheduleMenuAsync(botClient, chatId, telegramUserId, cancellationToken);
            return true;
        }

        if (session.CurrentStep == ConversationStep.MasterScheduleMenu &&
            MasterScheduleKeyboardBuilder.IsScheduleMenuButton(text))
        {
            await HandleScheduleMenuEntryAsync(
                botClient,
                chatId,
                telegramUserId,
                masterId,
                text,
                cancellationToken);
            return true;
        }

        return false;
    }

    public async Task SendPostRegisterOfferAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        CancellationToken cancellationToken = default)
    {
        await _sessionService.SetStepAsync(
            telegramUserId,
            chatId,
            ConversationStep.MasterPostRegisterScheduleOffer,
            cancellationToken);

        await botClient.SendMessage(
            chatId,
            TelegramBotTexts.MasterSchedule.PostRegisterOffer,
            replyMarkup: MasterScheduleKeyboardBuilder.BuildPostRegisterOffer(),
            cancellationToken: cancellationToken);
    }

    private async Task HandlePostRegisterOfferAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        string text,
        CancellationToken cancellationToken)
    {
        if (text == TelegramBotTexts.MasterSchedule.ButtonSetupLater)
        {
            await _sessionService.ReturnToMainMenuAsync(telegramUserId, chatId, cancellationToken);

            await botClient.SendMessage(
                chatId,
                MenuKeyboardBuilder.GetMenuTitle(TelegramUserRole.Master),
                replyMarkup: MenuKeyboardBuilder.BuildMasterMenu(),
                cancellationToken: cancellationToken);
            return;
        }

        if (text == TelegramBotTexts.MasterSchedule.ButtonSetupSchedule)
        {
            await botClient.SendMessage(
                chatId,
                TelegramBotTexts.MasterSchedule.PostRegisterHint,
                cancellationToken: cancellationToken);

            await ShowWorkingHoursListAsync(
                botClient,
                chatId,
                telegramUserId,
                await GetMasterIdAsync(telegramUserId, cancellationToken),
                cancellationToken);
            return;
        }

        await botClient.SendMessage(
            chatId,
            "Оберіть «Налаштувати робочі години» або «Пізніше».",
            replyMarkup: MasterScheduleKeyboardBuilder.BuildPostRegisterOffer(),
            cancellationToken: cancellationToken);
    }

    private async Task HandleScheduleMenuEntryAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        int masterId,
        string text,
        CancellationToken cancellationToken)
    {
        switch (text)
        {
            case TelegramBotTexts.MasterSchedule.ButtonWorkingHours:
                await ShowWorkingHoursListAsync(botClient, chatId, telegramUserId, masterId, cancellationToken);
                return;

            case TelegramBotTexts.MasterSchedule.ButtonBuffer:
                await StartBufferEditAsync(botClient, chatId, telegramUserId, masterId, cancellationToken);
                return;

            case TelegramBotTexts.MasterSchedule.ButtonSlotInterval:
                await StartSlotIntervalEditAsync(botClient, chatId, telegramUserId, masterId, cancellationToken);
                return;

            case TelegramBotTexts.MasterSchedule.ButtonTimeOff:
                await ShowTimeOffMenuAsync(botClient, chatId, telegramUserId, masterId, cancellationToken);
                return;
        }
    }

    private async Task HandleScheduleStepAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        int masterId,
        TelegramUserSession session,
        string text,
        CancellationToken cancellationToken)
    {
        switch (session.CurrentStep)
        {
            case ConversationStep.MasterScheduleMenu:
                if (MasterScheduleKeyboardBuilder.IsScheduleMenuButton(text))
                {
                    await HandleScheduleMenuEntryAsync(
                        botClient,
                        chatId,
                        telegramUserId,
                        masterId,
                        text,
                        cancellationToken);
                }
                else if (MasterScheduleKeyboardBuilder.IsScheduleSettingsEntryButton(text))
                {
                    await ShowScheduleMenuAsync(botClient, chatId, telegramUserId, cancellationToken);
                }
                else
                {
                    await PromptScheduleMenuAsync(botClient, chatId, cancellationToken);
                }

                return;

            case ConversationStep.MasterEditingWorkHoursPickDay:
                await HandleWorkHoursPickDayAsync(
                    botClient,
                    chatId,
                    telegramUserId,
                    masterId,
                    text,
                    cancellationToken);
                return;

            case ConversationStep.MasterEditingWorkHoursStart:
                await HandleWorkHoursStartAsync(
                    botClient,
                    chatId,
                    telegramUserId,
                    masterId,
                    session,
                    text,
                    cancellationToken);
                return;

            case ConversationStep.MasterEditingWorkHoursEnd:
                await HandleWorkHoursEndAsync(
                    botClient,
                    chatId,
                    telegramUserId,
                    masterId,
                    session,
                    text,
                    cancellationToken);
                return;

            case ConversationStep.MasterEditingBuffer:
                await HandleBufferInputAsync(
                    botClient,
                    chatId,
                    telegramUserId,
                    masterId,
                    text,
                    cancellationToken);
                return;

            case ConversationStep.MasterEditingSlotInterval:
                await HandleSlotIntervalInputAsync(
                    botClient,
                    chatId,
                    telegramUserId,
                    masterId,
                    text,
                    cancellationToken);
                return;

            case ConversationStep.MasterAddingTimeOffMenu:
                await HandleTimeOffMenuAsync(
                    botClient,
                    chatId,
                    telegramUserId,
                    masterId,
                    text,
                    cancellationToken);
                return;

            case ConversationStep.MasterAddingTimeOffPickDate:
                await HandleTimeOffPickDateAsync(
                    botClient,
                    chatId,
                    telegramUserId,
                    masterId,
                    session,
                    text,
                    cancellationToken);
                return;

            case ConversationStep.MasterAddingTimeOffPickMode:
                await HandleTimeOffPickModeAsync(
                    botClient,
                    chatId,
                    telegramUserId,
                    masterId,
                    session,
                    text,
                    cancellationToken);
                return;

            case ConversationStep.MasterAddingTimeOffStartTime:
                await HandleTimeOffStartTimeAsync(
                    botClient,
                    chatId,
                    telegramUserId,
                    session,
                    text,
                    cancellationToken);
                return;

            case ConversationStep.MasterAddingTimeOffEndTime:
                await HandleTimeOffEndTimeAsync(
                    botClient,
                    chatId,
                    telegramUserId,
                    masterId,
                    session,
                    text,
                    cancellationToken);
                return;

            case ConversationStep.MasterDeletingTimeOffPickNumber:
                await HandleTimeOffDeleteNumberAsync(
                    botClient,
                    chatId,
                    telegramUserId,
                    masterId,
                    session,
                    text,
                    cancellationToken);
                return;
        }
    }

    private async Task ShowScheduleMenuAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        CancellationToken cancellationToken)
    {
        await _sessionService.SetStepAsync(
            telegramUserId,
            chatId,
            ConversationStep.MasterScheduleMenu,
            cancellationToken);

        await botClient.SendMessage(
            chatId,
            TelegramBotTexts.MasterSchedule.MenuTitle,
            replyMarkup: MasterScheduleKeyboardBuilder.BuildScheduleMenu(),
            cancellationToken: cancellationToken);
    }

    private async Task PromptScheduleMenuAsync(
        ITelegramBotClient botClient,
        long chatId,
        CancellationToken cancellationToken) =>
        await botClient.SendMessage(
            chatId,
            TelegramBotTexts.MasterSchedule.MenuTitle,
            replyMarkup: MasterScheduleKeyboardBuilder.BuildScheduleMenu(),
            cancellationToken: cancellationToken);

    private async Task ShowWorkingHoursListAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        int masterId,
        CancellationToken cancellationToken)
    {
        var workingHours = await _scheduleSettingsService.GetWorkingHoursAsync(masterId, cancellationToken);
        var message = TelegramBotTexts.MasterSchedule.FormatWorkingHoursList(workingHours);

        await _sessionService.SetStepAsync(
            telegramUserId,
            chatId,
            ConversationStep.MasterEditingWorkHoursPickDay,
            cancellationToken);

        await botClient.SendMessage(
            chatId,
            TelegramBotTexts.MasterSchedule.WorkingHoursTitle + "\n\n" + message,
            replyMarkup: MasterScheduleKeyboardBuilder.BuildWeekdayPicker(),
            cancellationToken: cancellationToken);
    }

    private async Task HandleWorkHoursPickDayAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        int masterId,
        string text,
        CancellationToken cancellationToken)
    {
        if (!TelegramBotTexts.MasterSchedule.TryParseDayLabel(text, out var day))
        {
            await botClient.SendMessage(
                chatId,
                "Оберіть день з кнопок нижче.",
                replyMarkup: MasterScheduleKeyboardBuilder.BuildWeekdayPicker(),
                cancellationToken: cancellationToken);
            return;
        }

        var draft = new WorkHoursEditDraft { Day = day };

        await _sessionService.SaveWorkHoursEditDraftAsync(
            telegramUserId,
            chatId,
            draft,
            ConversationStep.MasterEditingWorkHoursStart,
            cancellationToken);

        var dayLabel = TelegramBotTexts.MasterSchedule.GetDayLabel(day);

        await botClient.SendMessage(
            chatId,
            TelegramBotTexts.MasterSchedule.PromptWorkStart(dayLabel),
            replyMarkup: MasterScheduleKeyboardBuilder.BuildWorkHoursStartInput(),
            cancellationToken: cancellationToken);
    }

    private async Task HandleWorkHoursStartAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        int masterId,
        TelegramUserSession session,
        string text,
        CancellationToken cancellationToken)
    {
        var draft = _sessionService.GetWorkHoursEditDraft(session);
        if (draft?.Day is not { } day)
        {
            await ShowWorkingHoursListAsync(botClient, chatId, telegramUserId, masterId, cancellationToken);
            return;
        }

        var dayLabel = TelegramBotTexts.MasterSchedule.GetDayLabel(day);

        if (text == TelegramBotTexts.MasterSchedule.ButtonNotWorking)
        {
            await _scheduleSettingsService.DeleteWorkingHoursForDayAsync(masterId, day, cancellationToken);

            await botClient.SendMessage(
                chatId,
                TelegramBotTexts.MasterSchedule.DayMarkedNotWorking,
                cancellationToken: cancellationToken);

            await ShowWorkingHoursListAsync(botClient, chatId, telegramUserId, masterId, cancellationToken);
            return;
        }

        if (!ScheduleInputParser.TryParseTime(text, out var startTime))
        {
            await botClient.SendMessage(
                chatId,
                TelegramBotTexts.MasterSchedule.InvalidTime,
                replyMarkup: MasterScheduleKeyboardBuilder.BuildWorkHoursStartInput(),
                cancellationToken: cancellationToken);
            return;
        }

        draft.WorkStartTime = startTime;

        await _sessionService.SaveWorkHoursEditDraftAsync(
            telegramUserId,
            chatId,
            draft,
            ConversationStep.MasterEditingWorkHoursEnd,
            cancellationToken);

        await botClient.SendMessage(
            chatId,
            TelegramBotTexts.MasterSchedule.PromptWorkEnd(dayLabel),
            replyMarkup: MenuKeyboardBuilder.BuildWithBackButton(),
            cancellationToken: cancellationToken);
    }

    private async Task HandleWorkHoursEndAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        int masterId,
        TelegramUserSession session,
        string text,
        CancellationToken cancellationToken)
    {
        var draft = _sessionService.GetWorkHoursEditDraft(session);
        if (draft?.Day is not { } day || draft.WorkStartTime is not { } startTime)
        {
            await ShowWorkingHoursListAsync(botClient, chatId, telegramUserId, masterId, cancellationToken);
            return;
        }

        if (!ScheduleInputParser.TryParseTime(text, out var endTime))
        {
            await botClient.SendMessage(
                chatId,
                TelegramBotTexts.MasterSchedule.InvalidTime,
                replyMarkup: MenuKeyboardBuilder.BuildWithBackButton(),
                cancellationToken: cancellationToken);
            return;
        }

        var result = await _scheduleSettingsService.UpsertWorkingHoursAsync(
            masterId,
            day,
            startTime,
            endTime,
            cancellationToken);

        if (!result.Success)
        {
            await botClient.SendMessage(
                chatId,
                result.ErrorMessage!,
                replyMarkup: MenuKeyboardBuilder.BuildWithBackButton(),
                cancellationToken: cancellationToken);
            return;
        }

        await botClient.SendMessage(
            chatId,
            TelegramBotTexts.MasterSchedule.WorkingHoursSaved,
            cancellationToken: cancellationToken);

        await ShowWorkingHoursListAsync(botClient, chatId, telegramUserId, masterId, cancellationToken);
    }

    private async Task StartBufferEditAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        int masterId,
        CancellationToken cancellationToken)
    {
        var setting = await _scheduleSettingsService.GetSettingAsync(masterId, cancellationToken);
        var current = setting?.BufferBetweenClientsMinutes ?? 0;

        await _sessionService.SetStepAsync(
            telegramUserId,
            chatId,
            ConversationStep.MasterEditingBuffer,
            cancellationToken);

        await botClient.SendMessage(
            chatId,
            TelegramBotTexts.MasterSchedule.PromptBuffer(current),
            replyMarkup: MenuKeyboardBuilder.BuildWithBackButton(),
            cancellationToken: cancellationToken);
    }

    private async Task HandleBufferInputAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        int masterId,
        string text,
        CancellationToken cancellationToken)
    {
        if (!ScheduleInputParser.TryParseBufferMinutes(text, out var minutes))
        {
            await botClient.SendMessage(
                chatId,
                TelegramBotTexts.MasterSchedule.InvalidBuffer,
                replyMarkup: MenuKeyboardBuilder.BuildWithBackButton(),
                cancellationToken: cancellationToken);
            return;
        }

        var result = await _scheduleSettingsService.UpdateBufferMinutesAsync(
            masterId,
            minutes,
            cancellationToken);

        if (!result.Success)
        {
            await botClient.SendMessage(
                chatId,
                result.ErrorMessage!,
                replyMarkup: MenuKeyboardBuilder.BuildWithBackButton(),
                cancellationToken: cancellationToken);
            return;
        }

        await _sessionService.SetStepAsync(
            telegramUserId,
            chatId,
            ConversationStep.MasterScheduleMenu,
            cancellationToken);

        await botClient.SendMessage(
            chatId,
            TelegramBotTexts.MasterSchedule.BufferSaved,
            replyMarkup: MasterScheduleKeyboardBuilder.BuildScheduleMenu(),
            cancellationToken: cancellationToken);
    }

    private async Task StartSlotIntervalEditAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        int masterId,
        CancellationToken cancellationToken)
    {
        var setting = await _scheduleSettingsService.GetSettingAsync(masterId, cancellationToken);
        var current = setting?.FreeSlotIntervalMinutes ?? 15;

        await _sessionService.SetStepAsync(
            telegramUserId,
            chatId,
            ConversationStep.MasterEditingSlotInterval,
            cancellationToken);

        await botClient.SendMessage(
            chatId,
            TelegramBotTexts.MasterSchedule.PromptSlotInterval(current),
            replyMarkup: MasterScheduleKeyboardBuilder.BuildSlotIntervalPicker(),
            cancellationToken: cancellationToken);
    }

    private async Task HandleSlotIntervalInputAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        int masterId,
        string text,
        CancellationToken cancellationToken)
    {
        if (!ScheduleInputParser.TryParseSlotIntervalMinutes(text, out var minutes))
        {
            await botClient.SendMessage(
                chatId,
                TelegramBotTexts.MasterSchedule.InvalidSlotInterval,
                replyMarkup: MasterScheduleKeyboardBuilder.BuildSlotIntervalPicker(),
                cancellationToken: cancellationToken);
            return;
        }

        var result = await _scheduleSettingsService.UpdateFreeSlotIntervalMinutesAsync(
            masterId,
            minutes,
            cancellationToken);

        if (!result.Success)
        {
            await botClient.SendMessage(
                chatId,
                result.ErrorMessage!,
                replyMarkup: MasterScheduleKeyboardBuilder.BuildSlotIntervalPicker(),
                cancellationToken: cancellationToken);
            return;
        }

        await _sessionService.SetStepAsync(
            telegramUserId,
            chatId,
            ConversationStep.MasterScheduleMenu,
            cancellationToken);

        await botClient.SendMessage(
            chatId,
            TelegramBotTexts.MasterSchedule.SlotIntervalSaved,
            replyMarkup: MasterScheduleKeyboardBuilder.BuildScheduleMenu(),
            cancellationToken: cancellationToken);
    }

    private async Task ShowTimeOffMenuAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        int masterId,
        CancellationToken cancellationToken)
    {
        var timeZone = await _availabilityService.GetMasterTimeZoneAsync(masterId, cancellationToken);
        var today = MasterTimeZoneHelper.ToLocalDate(DateTime.UtcNow, timeZone);
        var (fromUtc, _) = MasterTimeZoneHelper.GetDayBoundsUtc(today, timeZone);
        var (_, toUtc) = MasterTimeZoneHelper.GetDayBoundsUtc(today.AddDays(TimeOffListDaysAhead), timeZone);

        var timeOffs = await _scheduleSettingsService.GetTimeOffsInRangeAsync(
            masterId,
            fromUtc,
            toUtc,
            cancellationToken);

        var message = TelegramBotTexts.MasterSchedule.FormatTimeOffList(timeOffs, timeZone);

        await _sessionService.SetStepAsync(
            telegramUserId,
            chatId,
            ConversationStep.MasterAddingTimeOffMenu,
            cancellationToken);

        await botClient.SendMessage(
            chatId,
            message,
            replyMarkup: MasterScheduleKeyboardBuilder.BuildTimeOffMenu(),
            cancellationToken: cancellationToken);
    }

    private async Task HandleTimeOffMenuAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        int masterId,
        string text,
        CancellationToken cancellationToken)
    {
        switch (text)
        {
            case TelegramBotTexts.MasterSchedule.ButtonAddTimeOff:
                await StartAddTimeOffAsync(botClient, chatId, telegramUserId, cancellationToken);
                return;

            case TelegramBotTexts.MasterSchedule.ButtonDeleteTimeOff:
                await StartDeleteTimeOffAsync(
                    botClient,
                    chatId,
                    telegramUserId,
                    masterId,
                    cancellationToken);
                return;

            default:
                await ShowTimeOffMenuAsync(botClient, chatId, telegramUserId, masterId, cancellationToken);
                return;
        }
    }

    private async Task StartAddTimeOffAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        CancellationToken cancellationToken)
    {
        await _sessionService.SaveTimeOffDraftAsync(
            telegramUserId,
            chatId,
            new TimeOffDraft(),
            ConversationStep.MasterAddingTimeOffPickDate,
            cancellationToken);

        await botClient.SendMessage(
            chatId,
            TelegramBotTexts.MasterSchedule.PromptTimeOffDate,
            replyMarkup: MasterScheduleKeyboardBuilder.BuildTimeOffDatePicker(),
            cancellationToken: cancellationToken);
    }

    private async Task StartDeleteTimeOffAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        int masterId,
        CancellationToken cancellationToken)
    {
        var timeZone = await _availabilityService.GetMasterTimeZoneAsync(masterId, cancellationToken);
        var today = MasterTimeZoneHelper.ToLocalDate(DateTime.UtcNow, timeZone);
        var (fromUtc, _) = MasterTimeZoneHelper.GetDayBoundsUtc(today, timeZone);
        var (_, toUtc) = MasterTimeZoneHelper.GetDayBoundsUtc(today.AddDays(TimeOffListDaysAhead), timeZone);

        var timeOffs = await _scheduleSettingsService.GetTimeOffsInRangeAsync(
            masterId,
            fromUtc,
            toUtc,
            cancellationToken);

        if (timeOffs.Count == 0)
        {
            await botClient.SendMessage(
                chatId,
                TelegramBotTexts.MasterSchedule.TimeOffDeleteEmpty,
                replyMarkup: MasterScheduleKeyboardBuilder.BuildTimeOffMenu(),
                cancellationToken: cancellationToken);
            return;
        }

        var draft = new TimeOffDraft
        {
            ListedTimeOffIds = timeOffs.Select(x => x.Id).ToList()
        };

        await _sessionService.SaveTimeOffDraftAsync(
            telegramUserId,
            chatId,
            draft,
            ConversationStep.MasterDeletingTimeOffPickNumber,
            cancellationToken);

        var message = TelegramBotTexts.MasterSchedule.FormatTimeOffList(timeOffs, timeZone);

        await botClient.SendMessage(
            chatId,
            message + "\n\n" + TelegramBotTexts.MasterSchedule.PromptTimeOffDeleteNumber,
            replyMarkup: MenuKeyboardBuilder.BuildWithBackButton(),
            cancellationToken: cancellationToken);
    }

    private async Task HandleTimeOffPickDateAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        int masterId,
        TelegramUserSession session,
        string text,
        CancellationToken cancellationToken)
    {
        var timeZone = await _availabilityService.GetMasterTimeZoneAsync(masterId, cancellationToken);
        var today = MasterTimeZoneHelper.ToLocalDate(DateTime.UtcNow, timeZone);

        DateOnly date;
        if (text == TelegramBotTexts.MasterSchedule.ButtonToday)
        {
            date = today;
        }
        else if (text == TelegramBotTexts.MasterSchedule.ButtonTomorrow)
        {
            date = today.AddDays(1);
        }
        else if (!ScheduleInputParser.TryParseDate(text, today, out date))
        {
            await botClient.SendMessage(
                chatId,
                TelegramBotTexts.MasterSchedule.InvalidDate,
                replyMarkup: MasterScheduleKeyboardBuilder.BuildTimeOffDatePicker(),
                cancellationToken: cancellationToken);
            return;
        }

        var draft = _sessionService.GetTimeOffDraft(session) ?? new TimeOffDraft();
        draft.LocalDate = date;

        await _sessionService.SaveTimeOffDraftAsync(
            telegramUserId,
            chatId,
            draft,
            ConversationStep.MasterAddingTimeOffPickMode,
            cancellationToken);

        await botClient.SendMessage(
            chatId,
            TelegramBotTexts.MasterSchedule.PromptTimeOffMode,
            replyMarkup: MasterScheduleKeyboardBuilder.BuildTimeOffModePicker(),
            cancellationToken: cancellationToken);
    }

    private async Task HandleTimeOffPickModeAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        int masterId,
        TelegramUserSession session,
        string text,
        CancellationToken cancellationToken)
    {
        var draft = _sessionService.GetTimeOffDraft(session);
        if (draft?.LocalDate is not { } localDate)
        {
            await StartAddTimeOffAsync(botClient, chatId, telegramUserId, cancellationToken);
            return;
        }

        if (text == TelegramBotTexts.MasterSchedule.ButtonFullDay)
        {
            var timeZone = await _availabilityService.GetMasterTimeZoneAsync(masterId, cancellationToken);
            var (startUtc, endUtc) = MasterTimeZoneHelper.GetDayBoundsUtc(localDate, timeZone);

            var result = await _scheduleSettingsService.CreateTimeOffAsync(
                masterId,
                startUtc,
                endUtc,
                cancellationToken);

            if (!result.Success)
            {
                await botClient.SendMessage(
                    chatId,
                    result.ErrorMessage!,
                    replyMarkup: MasterScheduleKeyboardBuilder.BuildTimeOffModePicker(),
                    cancellationToken: cancellationToken);
                return;
            }

            await botClient.SendMessage(
                chatId,
                TelegramBotTexts.MasterSchedule.TimeOffCreated,
                cancellationToken: cancellationToken);

            await ShowTimeOffMenuAsync(botClient, chatId, telegramUserId, masterId, cancellationToken);
            return;
        }

        if (text == TelegramBotTexts.MasterSchedule.ButtonTimeInterval)
        {
            draft.IsFullDay = false;

            await _sessionService.SaveTimeOffDraftAsync(
                telegramUserId,
                chatId,
                draft,
                ConversationStep.MasterAddingTimeOffStartTime,
                cancellationToken);

            await botClient.SendMessage(
                chatId,
                TelegramBotTexts.MasterSchedule.PromptTimeOffStart,
                replyMarkup: MenuKeyboardBuilder.BuildWithBackButton(),
                cancellationToken: cancellationToken);
            return;
        }

        await botClient.SendMessage(
            chatId,
            TelegramBotTexts.MasterSchedule.PromptTimeOffMode,
            replyMarkup: MasterScheduleKeyboardBuilder.BuildTimeOffModePicker(),
            cancellationToken: cancellationToken);
    }

    private async Task HandleTimeOffStartTimeAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        TelegramUserSession session,
        string text,
        CancellationToken cancellationToken)
    {
        if (!ScheduleInputParser.TryParseTime(text, out var startTime))
        {
            await botClient.SendMessage(
                chatId,
                TelegramBotTexts.MasterSchedule.InvalidTime,
                replyMarkup: MenuKeyboardBuilder.BuildWithBackButton(),
                cancellationToken: cancellationToken);
            return;
        }

        var draft = _sessionService.GetTimeOffDraft(session) ?? new TimeOffDraft();
        draft.LocalStart = startTime;

        await _sessionService.SaveTimeOffDraftAsync(
            telegramUserId,
            chatId,
            draft,
            ConversationStep.MasterAddingTimeOffEndTime,
            cancellationToken);

        await botClient.SendMessage(
            chatId,
            TelegramBotTexts.MasterSchedule.PromptTimeOffEnd,
            replyMarkup: MenuKeyboardBuilder.BuildWithBackButton(),
            cancellationToken: cancellationToken);
    }

    private async Task HandleTimeOffEndTimeAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        int masterId,
        TelegramUserSession session,
        string text,
        CancellationToken cancellationToken)
    {
        var draft = _sessionService.GetTimeOffDraft(session);
        if (draft?.LocalDate is not { } localDate || draft.LocalStart is not { } localStart)
        {
            await StartAddTimeOffAsync(botClient, chatId, telegramUserId, cancellationToken);
            return;
        }

        if (!ScheduleInputParser.TryParseTime(text, out var localEnd))
        {
            await botClient.SendMessage(
                chatId,
                TelegramBotTexts.MasterSchedule.InvalidTime,
                replyMarkup: MenuKeyboardBuilder.BuildWithBackButton(),
                cancellationToken: cancellationToken);
            return;
        }

        var timeZone = await _availabilityService.GetMasterTimeZoneAsync(masterId, cancellationToken);
        var startLocal = localDate.ToDateTime(localStart);
        var endLocal = localDate.ToDateTime(localEnd);
        var startUtc = MasterTimeZoneHelper.ToUtc(startLocal, timeZone);
        var endUtc = MasterTimeZoneHelper.ToUtc(endLocal, timeZone);

        var result = await _scheduleSettingsService.CreateTimeOffAsync(
            masterId,
            startUtc,
            endUtc,
            cancellationToken);

        if (!result.Success)
        {
            await botClient.SendMessage(
                chatId,
                result.ErrorMessage!,
                replyMarkup: MenuKeyboardBuilder.BuildWithBackButton(),
                cancellationToken: cancellationToken);
            return;
        }

        await botClient.SendMessage(
            chatId,
            TelegramBotTexts.MasterSchedule.TimeOffCreated,
            cancellationToken: cancellationToken);

        await ShowTimeOffMenuAsync(botClient, chatId, telegramUserId, masterId, cancellationToken);
    }

    private async Task HandleTimeOffDeleteNumberAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        int masterId,
        TelegramUserSession session,
        string text,
        CancellationToken cancellationToken)
    {
        var draft = _sessionService.GetTimeOffDraft(session);
        var ids = draft?.ListedTimeOffIds;

        if (ids is null || ids.Count == 0)
        {
            await ShowTimeOffMenuAsync(botClient, chatId, telegramUserId, masterId, cancellationToken);
            return;
        }

        if (!ScheduleInputParser.TryParseListIndex(text, ids.Count, out var index))
        {
            await botClient.SendMessage(
                chatId,
                TelegramBotTexts.MasterSchedule.InvalidTimeOffNumber,
                replyMarkup: MenuKeyboardBuilder.BuildWithBackButton(),
                cancellationToken: cancellationToken);
            return;
        }

        var timeOffId = ids[index - 1];
        await _scheduleSettingsService.DeleteTimeOffAsync(timeOffId, cancellationToken);

        await botClient.SendMessage(
            chatId,
            TelegramBotTexts.MasterSchedule.TimeOffDeleted,
            cancellationToken: cancellationToken);

        await ShowTimeOffMenuAsync(botClient, chatId, telegramUserId, masterId, cancellationToken);
    }

    private async Task ShowScheduleViewMenuAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        CancellationToken cancellationToken)
    {
        await _sessionService.SetStepAsync(
            telegramUserId,
            chatId,
            ConversationStep.MasterScheduleViewMenu,
            cancellationToken);

        await botClient.SendMessage(
            chatId,
            TelegramBotTexts.MasterSchedule.ViewMenuTitle,
            replyMarkup: MasterScheduleKeyboardBuilder.BuildViewMenu(),
            cancellationToken: cancellationToken);
    }

    private async Task HandleScheduleViewMenuAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        int masterId,
        string text,
        CancellationToken cancellationToken)
    {
        if (!MasterScheduleKeyboardBuilder.IsViewPeriodButton(text))
        {
            await ShowScheduleViewMenuAsync(botClient, chatId, telegramUserId, cancellationToken);
            return;
        }

        var period = text switch
        {
            TelegramBotTexts.MasterSchedule.ButtonViewTomorrow => ScheduleViewPeriod.Tomorrow,
            TelegramBotTexts.MasterSchedule.ButtonViewThreeDays => ScheduleViewPeriod.ThreeDays,
            TelegramBotTexts.MasterSchedule.ButtonViewWeek => ScheduleViewPeriod.Week,
            TelegramBotTexts.MasterSchedule.ButtonViewMonth => ScheduleViewPeriod.Month,
            _ => ScheduleViewPeriod.Tomorrow
        };

        await SendScheduleViewAsync(
            botClient,
            chatId,
            telegramUserId,
            masterId,
            period,
            cancellationToken);
    }

    private async Task SendScheduleViewAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        int masterId,
        ScheduleViewPeriod period,
        CancellationToken cancellationToken)
    {
        var view = await _scheduleViewService.BuildViewAsync(masterId, period, cancellationToken);

        var draft = new ScheduleViewDraft
        {
            Appointments = view.Appointments.ToList()
        };

        await _sessionService.SaveScheduleViewDraftAsync(
            telegramUserId,
            chatId,
            draft,
            ConversationStep.MasterViewingAppointmentPickNumber,
            cancellationToken);

        var keyboard = view.Appointments.Count > 0
            ? MenuKeyboardBuilder.BuildWithBackButton()
            : MasterScheduleKeyboardBuilder.BuildViewMenu();

        for (var i = 0; i < view.MessageParts.Count; i++)
        {
            var isLast = i == view.MessageParts.Count - 1;
            await botClient.SendMessage(
                chatId,
                view.MessageParts[i],
                replyMarkup: isLast ? keyboard : null,
                cancellationToken: cancellationToken);
        }
    }

    private async Task HandleAppointmentPickNumberAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        int masterId,
        TelegramUserSession session,
        string text,
        CancellationToken cancellationToken)
    {
        if (MasterScheduleKeyboardBuilder.IsViewPeriodButton(text))
        {
            await HandleScheduleViewMenuAsync(
                botClient,
                chatId,
                telegramUserId,
                masterId,
                text,
                cancellationToken);
            return;
        }

        var draft = _sessionService.GetScheduleViewDraft(session);
        var appointments = draft?.Appointments ?? [];

        if (appointments.Count == 0)
        {
            await ShowScheduleViewMenuAsync(botClient, chatId, telegramUserId, cancellationToken);
            return;
        }

        if (!ScheduleInputParser.TryParseListIndex(text, appointments.Count, out var index))
        {
            await botClient.SendMessage(
                chatId,
                TelegramBotTexts.MasterSchedule.InvalidAppointmentNumber,
                replyMarkup: MenuKeyboardBuilder.BuildWithBackButton(),
                cancellationToken: cancellationToken);
            return;
        }

        var appointmentId = appointments[index - 1].AppointmentId;
        var details = await _scheduleViewService.BuildAppointmentDetailsAsync(
            masterId,
            appointmentId,
            cancellationToken);

        if (details is null)
        {
            await botClient.SendMessage(
                chatId,
                TelegramBotTexts.MasterSchedule.AppointmentNotFound,
                replyMarkup: MenuKeyboardBuilder.BuildWithBackButton(),
                cancellationToken: cancellationToken);
            return;
        }

        await botClient.SendMessage(
            chatId,
            details,
            replyMarkup: MenuKeyboardBuilder.BuildWithBackButton(),
            cancellationToken: cancellationToken);
    }

    private async Task<int> GetMasterIdAsync(long telegramUserId, CancellationToken cancellationToken)
    {
        var session = await _sessionService.GetAsync(telegramUserId, cancellationToken);
        return session?.MasterId
            ?? throw new InvalidOperationException("Master session was not found.");
    }

    private static bool IsAtMasterMainMenu(ConversationStep step) =>
        step is ConversationStep.Idle or ConversationStep.MasterPanelMenu;

    private static bool IsClientBookingStep(ConversationStep step) =>
        step is ConversationStep.ClientChoosingBookPeriod
            or ConversationStep.ClientChoosingBookService
            or ConversationStep.ClientChoosingBookDate
            or ConversationStep.ClientChoosingBookSlot
            or ConversationStep.ClientConfirmingBooking;

    private static bool IsMasterOnlyScheduleButton(string text) =>
        MasterScheduleKeyboardBuilder.IsMyScheduleEntryButton(text)
        || MasterScheduleKeyboardBuilder.IsScheduleSettingsEntryButton(text)
        || MasterScheduleKeyboardBuilder.IsScheduleMenuButton(text)
        || MasterScheduleKeyboardBuilder.IsSlotIntervalPresetButton(text)
        || MasterScheduleKeyboardBuilder.IsPostRegisterSetupButton(text);
}
