using Business_Processes_Automation.BLL.DTOs.Schedule;
using Business_Processes_Automation.BLL.Enums;
using Business_Processes_Automation.BLL.SessionDrafts;

namespace Business_Processes_Automation.BLL.Services;

public interface IClientBookingService
{
    Task<ClientBookingViewResult> BuildOverviewAsync(
        int masterId,
        ScheduleViewPeriod period,
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
