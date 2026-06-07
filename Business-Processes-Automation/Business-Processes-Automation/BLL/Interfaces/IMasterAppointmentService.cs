using Business_Processes_Automation.BLL.Results;
using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.Interfaces;

public interface IMasterAppointmentService
{
    Task<AppointmentResponseDTO?> GetByIdAsync(
        int masterId,
        int appointmentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AppointmentResponseDTO>> ListAsync(
        int masterId,
        DateOnly from,
        DateOnly to,
        AppointmentStatus? status,
        int? serviceId,
        CancellationToken cancellationToken = default);

    Task<AppointmentOperationResult> CreateManualAsync(
        int masterId,
        CreateAppointmentRequestDTO request,
        CancellationToken cancellationToken = default);

    Task<AppointmentOperationResult> CreateFromTelegramAsync(
        int masterId,
        long telegramUserId,
        string clientDisplayName,
        int serviceId,
        FreeSlot slot,
        CancellationToken cancellationToken = default);

    Task<AppointmentOperationResult> UpdateAsync(
        int masterId,
        int appointmentId,
        UpdateAppointmentRequestDTO request,
        CancellationToken cancellationToken = default);

    Task<AppointmentOperationResult> CancelAsync(
        int masterId,
        int appointmentId,
        CancellationToken cancellationToken = default);

    Task<AppointmentOperationResult> RescheduleAsync(
        int masterId,
        int appointmentId,
        RescheduleAppointmentRequestDTO request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FreeSlot>> GetFreeSlotsForServiceAsync(
        int masterId,
        DateOnly from,
        DateOnly to,
        int serviceId,
        CancellationToken cancellationToken = default);

    Task<FreeSlotsResponseDTO> GetFreeSlotsResponseAsync(
        int masterId,
        DateOnly from,
        DateOnly to,
        int serviceId,
        CancellationToken cancellationToken = default);
}
