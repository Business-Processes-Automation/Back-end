using Business_Processes_Automation.BLL.Enums;
using Business_Processes_Automation.BLL.Helpers;
using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.BLL.Services;
using Business_Processes_Automation.BLL.SessionDrafts;
using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;
using Business_Processes_Automation.Telegram.Keyboards;
using Business_Processes_Automation.Telegram.Localization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Business_Processes_Automation.Telegram.Handlers.Messages;

public class ClientBookingHandler
{
    private readonly ITelegramUserSessionService _sessionService;
    private readonly IClientBookingService _bookingService;
    private readonly IMasterAvailabilityService _availabilityService;
    private readonly IServiceRepository _serviceRepository;

    public ClientBookingHandler(
        ITelegramUserSessionService sessionService,
        IClientBookingService bookingService,
        IMasterAvailabilityService availabilityService,
        IServiceRepository serviceRepository)
    {
        _sessionService = sessionService;
        _bookingService = bookingService;
        _availabilityService = availabilityService;
        _serviceRepository = serviceRepository;
    }

    public async Task<bool> TryHandleAsync(
        ITelegramBotClient botClient,
        Message message,
        long telegramUserId,
        string text,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionService.GetAsync(telegramUserId, cancellationToken);

        if (session?.MasterId is not { } masterId || session.Role != TelegramUserRole.Client)
        {
            return false;
        }

        var chatId = message.Chat.Id;

        if (MenuKeyboardBuilder.IsBackToMenu(text))
        {
            return false;
        }

        if (IsBookingStep(session.CurrentStep))
        {
            await HandleBookingStepAsync(
                botClient,
                message.From,
                chatId,
                telegramUserId,
                masterId,
                session,
                text,
                cancellationToken);
            return true;
        }

        if (!IsBookingStep(session.CurrentStep) && text == TelegramBotTexts.Menu.ButtonBook)
        {
            await StartBookingAsync(botClient, chatId, telegramUserId, masterId, cancellationToken);
            return true;
        }

        return false;
    }

    private async Task StartBookingAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        int masterId,
        CancellationToken cancellationToken)
    {
        var services = await _serviceRepository.GetByMasterIdAsync(masterId, cancellationToken);
        if (services.Count == 0)
        {
            await botClient.SendMessage(
                chatId,
                TelegramBotTexts.ClientBooking.NoServices,
                replyMarkup: MenuKeyboardBuilder.BuildClientMenu(),
                cancellationToken: cancellationToken);
            return;
        }

        await _sessionService.SetStepAsync(
            telegramUserId,
            chatId,
            ConversationStep.ClientChoosingBookPeriod,
            cancellationToken);

        await botClient.SendMessage(
            chatId,
            TelegramBotTexts.ClientBooking.PeriodMenuTitle,
            replyMarkup: ClientBookingKeyboardBuilder.BuildPeriodMenu(),
            cancellationToken: cancellationToken);
    }

    private async Task HandleBookingStepAsync(
        ITelegramBotClient botClient,
        User? from,
        long chatId,
        long telegramUserId,
        int masterId,
        TelegramUserSession session,
        string text,
        CancellationToken cancellationToken)
    {
        switch (session.CurrentStep)
        {
            case ConversationStep.ClientChoosingBookPeriod:
                await HandlePeriodChoiceAsync(botClient, chatId, telegramUserId, masterId, text, cancellationToken);
                return;

            case ConversationStep.ClientChoosingBookService:
                await HandleServiceChoiceAsync(
                    botClient,
                    chatId,
                    telegramUserId,
                    masterId,
                    session,
                    text,
                    cancellationToken);
                return;

            case ConversationStep.ClientChoosingBookDate:
                await HandleDateChoiceAsync(
                    botClient,
                    chatId,
                    telegramUserId,
                    masterId,
                    session,
                    text,
                    cancellationToken);
                return;

            case ConversationStep.ClientChoosingBookSlot:
                await HandleSlotChoiceAsync(
                    botClient,
                    chatId,
                    telegramUserId,
                    masterId,
                    session,
                    text,
                    cancellationToken);
                return;

            case ConversationStep.ClientConfirmingBooking:
                await HandleConfirmationAsync(
                    botClient,
                    from,
                    chatId,
                    telegramUserId,
                    masterId,
                    session,
                    text,
                    cancellationToken);
                return;
        }
    }

    private async Task HandlePeriodChoiceAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        int masterId,
        string text,
        CancellationToken cancellationToken)
    {
        if (!ClientBookingKeyboardBuilder.IsViewPeriodButton(text))
        {
            await botClient.SendMessage(
                chatId,
                TelegramBotTexts.ClientBooking.PeriodMenuTitle,
                replyMarkup: ClientBookingKeyboardBuilder.BuildPeriodMenu(),
                cancellationToken: cancellationToken);
            return;
        }

        var period = MapPeriod(text);
        var timeZone = await _availabilityService.GetMasterTimeZoneAsync(masterId, cancellationToken);
        var today = MasterTimeZoneHelper.ToLocalDate(DateTime.UtcNow, timeZone);
        var (rangeStart, rangeEnd) = SchedulePeriodHelper.GetDateRange(period, today);

        var services = await _serviceRepository.GetByMasterIdAsync(masterId, cancellationToken);

        var draft = new BookingDraft
        {
            Period = period,
            RangeStart = rangeStart,
            RangeEnd = rangeEnd,
            ServiceIdsInOrder = services.Select(x => x.Id).ToList()
        };

        var view = await _bookingService.BuildPeriodIntroAsync(masterId, period, cancellationToken);

        await _sessionService.SaveBookingDraftAsync(
            telegramUserId,
            chatId,
            draft,
            ConversationStep.ClientChoosingBookService,
            cancellationToken);

        await SendViewPartsAsync(
            botClient,
            chatId,
            view.MessageParts,
            ClientBookingKeyboardBuilder.BuildServiceMenu(services),
            cancellationToken);
    }

    private async Task HandleServiceChoiceAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        int masterId,
        TelegramUserSession session,
        string text,
        CancellationToken cancellationToken)
    {
        if (ClientBookingKeyboardBuilder.IsViewPeriodButton(text))
        {
            await HandlePeriodChoiceAsync(botClient, chatId, telegramUserId, masterId, text, cancellationToken);
            return;
        }

        var draft = _sessionService.GetBookingDraft(session);
        if (draft is null || draft.ServiceIdsInOrder.Count == 0)
        {
            await StartBookingAsync(botClient, chatId, telegramUserId, masterId, cancellationToken);
            return;
        }

        var services = await _serviceRepository.GetByMasterIdAsync(masterId, cancellationToken);
        var service = ClientBookingKeyboardBuilder.TryMatchService(text, services);
        if (service is null)
        {
            await botClient.SendMessage(
                chatId,
                TelegramBotTexts.ClientBooking.InvalidService,
                replyMarkup: ClientBookingKeyboardBuilder.BuildServiceMenu(services),
                cancellationToken: cancellationToken);
            return;
        }

        draft.ServiceId = service.Id;
        draft.SelectedDate = null;
        draft.Slots = [];

        if (draft.Period == ScheduleViewPeriod.Tomorrow)
        {
            draft.SelectedDate = draft.RangeStart;
            await _sessionService.SaveBookingDraftAsync(
                telegramUserId,
                chatId,
                draft,
                ConversationStep.ClientChoosingBookSlot,
                cancellationToken);

            await ShowDaySlotsAsync(
                botClient,
                chatId,
                masterId,
                draft,
                service.Id,
                draft.RangeStart,
                showBackToDates: false,
                cancellationToken);
            return;
        }

        var dates = await _bookingService.GetDatesWithFreeSlotsAsync(masterId, draft, service.Id, cancellationToken);
        if (dates.Count == 0)
        {
            await botClient.SendMessage(
                chatId,
                TelegramBotTexts.ClientBooking.NoSlotsInPeriod,
                replyMarkup: ClientBookingKeyboardBuilder.BuildPeriodMenu(),
                cancellationToken: cancellationToken);
            return;
        }

        await _sessionService.SaveBookingDraftAsync(
            telegramUserId,
            chatId,
            draft,
            ConversationStep.ClientChoosingBookDate,
            cancellationToken);

        await botClient.SendMessage(
            chatId,
            $"{service.ServiceName}\n\n{TelegramBotTexts.ClientBooking.ChooseDate}",
            replyMarkup: ClientBookingKeyboardBuilder.BuildDateMenu(dates),
            cancellationToken: cancellationToken);
    }

    private async Task HandleDateChoiceAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        int masterId,
        TelegramUserSession session,
        string text,
        CancellationToken cancellationToken)
    {
        if (ClientBookingKeyboardBuilder.IsViewPeriodButton(text))
        {
            await HandlePeriodChoiceAsync(botClient, chatId, telegramUserId, masterId, text, cancellationToken);
            return;
        }

        var draft = _sessionService.GetBookingDraft(session);
        if (draft?.ServiceId is not { } serviceId)
        {
            await StartBookingAsync(botClient, chatId, telegramUserId, masterId, cancellationToken);
            return;
        }

        var dates = await _bookingService.GetDatesWithFreeSlotsAsync(masterId, draft, serviceId, cancellationToken);
        var selectedDate = ClientBookingKeyboardBuilder.TryMatchDate(text, dates);
        if (selectedDate is null)
        {
            await botClient.SendMessage(
                chatId,
                TelegramBotTexts.ClientBooking.InvalidDate,
                replyMarkup: ClientBookingKeyboardBuilder.BuildDateMenu(dates),
                cancellationToken: cancellationToken);
            return;
        }

        draft.SelectedDate = selectedDate.Value;

        await _sessionService.SaveBookingDraftAsync(
            telegramUserId,
            chatId,
            draft,
            ConversationStep.ClientChoosingBookSlot,
            cancellationToken);

        await ShowDaySlotsAsync(
            botClient,
            chatId,
            masterId,
            draft,
            serviceId,
            selectedDate.Value,
            showBackToDates: true,
            cancellationToken);
    }

    private async Task HandleSlotChoiceAsync(
        ITelegramBotClient botClient,
        long chatId,
        long telegramUserId,
        int masterId,
        TelegramUserSession session,
        string text,
        CancellationToken cancellationToken)
    {
        if (ClientBookingKeyboardBuilder.IsViewPeriodButton(text))
        {
            await HandlePeriodChoiceAsync(botClient, chatId, telegramUserId, masterId, text, cancellationToken);
            return;
        }

        var draft = _sessionService.GetBookingDraft(session);
        if (draft?.ServiceId is not { } serviceId || draft.SelectedDate is not { } selectedDate)
        {
            await StartBookingAsync(botClient, chatId, telegramUserId, masterId, cancellationToken);
            return;
        }

        var showBackToDates = draft.Period != ScheduleViewPeriod.Tomorrow;

        if (ClientBookingKeyboardBuilder.IsBackToDates(text) && showBackToDates)
        {
            var dates = await _bookingService.GetDatesWithFreeSlotsAsync(masterId, draft, serviceId, cancellationToken);
            await _sessionService.SaveBookingDraftAsync(
                telegramUserId,
                chatId,
                draft,
                ConversationStep.ClientChoosingBookDate,
                cancellationToken);

            var service = (await _serviceRepository.GetByMasterIdAsync(masterId, cancellationToken))
                .First(x => x.Id == serviceId);

            await botClient.SendMessage(
                chatId,
                $"{service.ServiceName}\n\n{TelegramBotTexts.ClientBooking.ChooseDate}",
                replyMarkup: ClientBookingKeyboardBuilder.BuildDateMenu(dates),
                cancellationToken: cancellationToken);
            return;
        }

        var daySlots = await _bookingService.GetFreeSlotsForDayAsync(
            masterId,
            draft,
            serviceId,
            selectedDate,
            cancellationToken);

        if (daySlots.Count == 0)
        {
            await botClient.SendMessage(
                chatId,
                TelegramBotTexts.ClientBooking.NoSlotsOnDay,
                replyMarkup: showBackToDates
                    ? ClientBookingKeyboardBuilder.BuildDateMenu(
                        await _bookingService.GetDatesWithFreeSlotsAsync(masterId, draft, serviceId, cancellationToken))
                    : ClientBookingKeyboardBuilder.BuildPeriodMenu(),
                cancellationToken: cancellationToken);
            return;
        }

        var slotTimes = daySlots.Select(x => x.StartTime).ToList();
        var matchedTime = ClientBookingKeyboardBuilder.TryMatchSlotTime(text, slotTimes);
        if (matchedTime is null)
        {
            await botClient.SendMessage(
                chatId,
                TelegramBotTexts.ClientBooking.InvalidSlot,
                replyMarkup: ClientBookingKeyboardBuilder.BuildSlotMenu(slotTimes, showBackToDates),
                cancellationToken: cancellationToken);
            return;
        }

        var slot = daySlots.First(x => x.StartTime == matchedTime.Value);
        draft.Slots = [slot];

        var serviceEntity = (await _serviceRepository.GetByMasterIdAsync(masterId, cancellationToken))
            .First(x => x.Id == serviceId);

        var timeZone = await _availabilityService.GetMasterTimeZoneAsync(masterId, cancellationToken);
        var confirmText = ClientBookingViewFormatter.FormatConfirmation(
            serviceEntity,
            slot,
            timeZone,
            serviceEntity.Price,
            0);

        await _sessionService.SaveBookingDraftAsync(
            telegramUserId,
            chatId,
            draft,
            ConversationStep.ClientConfirmingBooking,
            cancellationToken);

        await botClient.SendMessage(
            chatId,
            confirmText,
            replyMarkup: ClientBookingKeyboardBuilder.BuildConfirmation(),
            cancellationToken: cancellationToken);
    }

    private async Task ShowDaySlotsAsync(
        ITelegramBotClient botClient,
        long chatId,
        int masterId,
        BookingDraft draft,
        int serviceId,
        DateOnly date,
        bool showBackToDates,
        CancellationToken cancellationToken)
    {
        var (view, daySlots) = await _bookingService.BuildDaySlotsViewAsync(
            masterId,
            draft,
            serviceId,
            date,
            cancellationToken);

        var keyboard = daySlots.Count > 0
            ? ClientBookingKeyboardBuilder.BuildSlotMenu(
                daySlots.Select(x => x.StartTime).ToList(),
                showBackToDates)
            : showBackToDates
                ? ClientBookingKeyboardBuilder.BuildDateMenu(
                    await _bookingService.GetDatesWithFreeSlotsAsync(masterId, draft, serviceId, cancellationToken))
                : ClientBookingKeyboardBuilder.BuildPeriodMenu();

        await SendViewPartsAsync(botClient, chatId, view.MessageParts, keyboard, cancellationToken);
    }

    private async Task HandleConfirmationAsync(
        ITelegramBotClient botClient,
        User? from,
        long chatId,
        long telegramUserId,
        int masterId,
        TelegramUserSession session,
        string text,
        CancellationToken cancellationToken)
    {
        if (ClientBookingKeyboardBuilder.IsCancel(text))
        {
            await _sessionService.ReturnToMainMenuAsync(telegramUserId, chatId, cancellationToken);

            await botClient.SendMessage(
                chatId,
                TelegramBotTexts.ClientBooking.BookingCancelled,
                replyMarkup: MenuKeyboardBuilder.BuildClientMenu(),
                cancellationToken: cancellationToken);
            return;
        }

        if (!ClientBookingKeyboardBuilder.IsConfirm(text))
        {
            await botClient.SendMessage(
                chatId,
                TelegramBotTexts.ClientBooking.UseButtons,
                replyMarkup: ClientBookingKeyboardBuilder.BuildConfirmation(),
                cancellationToken: cancellationToken);
            return;
        }

        var draft = _sessionService.GetBookingDraft(session);
        if (draft?.ServiceId is not { } serviceId || draft.Slots.Count == 0)
        {
            await StartBookingAsync(botClient, chatId, telegramUserId, masterId, cancellationToken);
            return;
        }

        var displayName = BuildClientDisplayName(from);
        var result = await _bookingService.CreateBookingAsync(
            masterId,
            telegramUserId,
            displayName,
            serviceId,
            draft.Slots[0],
            cancellationToken);

        if (!result.Success)
        {
            await botClient.SendMessage(
                chatId,
                result.ErrorMessage ?? TelegramBotTexts.Common.UnexpectedError,
                replyMarkup: ClientBookingKeyboardBuilder.BuildConfirmation(),
                cancellationToken: cancellationToken);
            return;
        }

        await _sessionService.ReturnToMainMenuAsync(telegramUserId, chatId, cancellationToken);

        await botClient.SendMessage(
            chatId,
            TelegramBotTexts.ClientBooking.BookingSuccess,
            replyMarkup: MenuKeyboardBuilder.BuildClientMenu(),
            cancellationToken: cancellationToken);
    }

    private static async Task SendViewPartsAsync(
        ITelegramBotClient botClient,
        long chatId,
        IReadOnlyList<string> parts,
        ReplyMarkup? lastKeyboard,
        CancellationToken cancellationToken)
    {
        for (var i = 0; i < parts.Count; i++)
        {
            var isLast = i == parts.Count - 1;
            await botClient.SendMessage(
                chatId,
                parts[i],
                replyMarkup: isLast ? lastKeyboard : null,
                cancellationToken: cancellationToken);
        }
    }

    private static ScheduleViewPeriod MapPeriod(string text) =>
        text switch
        {
            TelegramBotTexts.MasterSchedule.ButtonViewTomorrow => ScheduleViewPeriod.Tomorrow,
            TelegramBotTexts.MasterSchedule.ButtonViewThreeDays => ScheduleViewPeriod.ThreeDays,
            TelegramBotTexts.MasterSchedule.ButtonViewWeek => ScheduleViewPeriod.Week,
            TelegramBotTexts.MasterSchedule.ButtonViewMonth => ScheduleViewPeriod.Month,
            _ => ScheduleViewPeriod.Tomorrow
        };

    private static string BuildClientDisplayName(User? user)
    {
        if (user is null)
        {
            return "Клієнт";
        }

        var parts = new[] { user.FirstName, user.LastName }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim());

        var name = string.Join(' ', parts);
        return string.IsNullOrWhiteSpace(name) ? user.Username ?? "Клієнт" : name;
    }

    private static bool IsBookingStep(ConversationStep step) =>
        step is ConversationStep.ClientChoosingBookPeriod
            or ConversationStep.ClientChoosingBookService
            or ConversationStep.ClientChoosingBookDate
            or ConversationStep.ClientChoosingBookSlot
            or ConversationStep.ClientConfirmingBooking;
}
