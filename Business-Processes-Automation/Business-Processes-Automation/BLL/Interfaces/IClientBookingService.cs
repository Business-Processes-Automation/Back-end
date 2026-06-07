using Business_Processes_Automation.BLL.Enums;
using Business_Processes_Automation.BLL.Results;
using Business_Processes_Automation.BLL.SessionDrafts;

namespace Business_Processes_Automation.BLL.Services;

public interface IClientBookingService
{
    Task<ClientBookingViewResult> BuildPeriodIntroAsync(
        int masterId,
        ScheduleViewPeriod period,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DateOnly>> GetDatesWithFreeSlotsAsync(
        int masterId,
        BookingDraft draft,
        int serviceId,
        CancellationToken cancellationToken = default);

    Task<(ClientBookingViewResult View, IReadOnlyList<FreeSlot> Slots)> BuildDaySlotsViewAsync(
        int masterId,
        BookingDraft draft,
        int serviceId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FreeSlot>> GetFreeSlotsForDayAsync(
        int masterId,
        BookingDraft draft,
        int serviceId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    Task<(ClientBookingViewResult View, IReadOnlyList<FreeSlot> Slots)> BuildSlotsViewAsync(
        int masterId,
        BookingDraft draft,
        int serviceId,
        CancellationToken cancellationToken = default);

    Task<BookingResult> CreateBookingAsync(
        int masterId,
        long telegramUserId,
        string clientDisplayName,
        int serviceId,
        FreeSlot slot,
        CancellationToken cancellationToken = default);
}
