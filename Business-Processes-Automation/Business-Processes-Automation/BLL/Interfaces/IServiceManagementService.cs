using Business_Processes_Automation.BLL.DTOs.Service;

namespace Business_Processes_Automation.BLL.Interfaces;

public interface IServiceManagementService
{
    Task<IReadOnlyList<ServiceResponseDTO>> GetByMasterIdAsync(
        int masterId,
        CancellationToken cancellationToken = default);

    Task<ServiceResponseDTO?> GetByIdAsync(
        int masterId,
        int serviceId,
        CancellationToken cancellationToken = default);

    Task<ServiceResponseDTO> CreateAsync(
        int masterId,
        CreateServiceRequestDTO request,
        CancellationToken cancellationToken = default);

    Task<ServiceResponseDTO?> UpdateAsync(
        int masterId,
        int serviceId,
        UpdateServiceRequestDTO request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        int masterId,
        int serviceId,
        CancellationToken cancellationToken = default);
}
