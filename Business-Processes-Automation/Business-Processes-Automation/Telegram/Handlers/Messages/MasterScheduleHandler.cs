using Business_Processes_Automation.BLL.Enums;
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
    private readonly ITelegramUserSessionService _sessionService;
    private readonly IMasterScheduleViewService _scheduleViewService;

    public MasterScheduleHandler(
        ITelegramUserSessionService sessionService,
        IMasterScheduleViewService scheduleViewService)
    {
        _sessionService = sessionService;
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

            if (MasterScheduleKeyboardBuilder.IsMyScheduleEntryButton(text))
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

        if (IsAtMasterMainMenu(session.CurrentStep) &&
            MasterScheduleKeyboardBuilder.IsMyScheduleEntryButton(text))
        {
            await ShowScheduleViewMenuAsync(botClient, chatId, telegramUserId, cancellationToken);
            return true;
        }

        return false;
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

    private static bool IsAtMasterMainMenu(ConversationStep step) =>
        step is ConversationStep.Idle;

    private static bool IsClientBookingStep(ConversationStep step) =>
        step is ConversationStep.ClientChoosingBookPeriod
            or ConversationStep.ClientChoosingBookService
            or ConversationStep.ClientChoosingBookDate
            or ConversationStep.ClientChoosingBookSlot
            or ConversationStep.ClientConfirmingBooking;
}
