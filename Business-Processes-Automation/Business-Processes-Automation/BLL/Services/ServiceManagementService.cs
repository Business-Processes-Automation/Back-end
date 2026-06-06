using Business_Processes_Automation.BLL.DTOs.Service;
using Business_Processes_Automation.BLL.Interfaces;
using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.BLL.Localization;
using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.BLL.Services;

public class ServiceManagementService : IServiceManagementService
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IAppointmentRepository _appointmentRepository;

    public ServiceManagementService(
        IServiceRepository serviceRepository,
        IAppointmentRepository appointmentRepository)
    {
        _serviceRepository = serviceRepository;
        _appointmentRepository = appointmentRepository;
    }

    public async Task<IReadOnlyList<ServiceResponseDTO>> GetByMasterIdAsync(
        int masterId,
        CancellationToken cancellationToken = default)
    {
        var services = await _serviceRepository.GetByMasterIdAsync(masterId, cancellationToken);
        return services.Select(MapToDto).ToList();
    }

    public async Task<ServiceResponseDTO?> GetByIdAsync(
        int masterId,
        int serviceId,
        CancellationToken cancellationToken = default)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceId, masterId, cancellationToken);
        return service is null ? null : MapToDto(service);
    }

    public async Task<ServiceResponseDTO> CreateAsync(
        int masterId,
        CreateServiceRequestDTO request,
        CancellationToken cancellationToken = default)
    {
        var name = request.ServiceName.Trim();
        await EnsureUniqueNameAsync(masterId, name, excludeServiceId: null, cancellationToken);

        var service = new Service { MasterId = masterId };
        ApplyRequest(service, request, name);

        var created = await _serviceRepository.CreateAsync(service, cancellationToken);
        return MapToDto(created);
    }

    public async Task<ServiceResponseDTO?> UpdateAsync(
        int masterId,
        int serviceId,
        UpdateServiceRequestDTO request,
        CancellationToken cancellationToken = default)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceId, masterId, cancellationToken);
        if (service is null)
        {
            return null;
        }

        var name = request.ServiceName.Trim();
        await EnsureUniqueNameAsync(masterId, name, serviceId, cancellationToken);

        ApplyRequest(service, request, name);

        var updated = await _serviceRepository.UpdateAsync(service, cancellationToken);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(
        int masterId,
        int serviceId,
        CancellationToken cancellationToken = default)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceId, masterId, cancellationToken);
        if (service is null)
        {
            return false;
        }

        if (await _appointmentRepository.HasActiveByServiceIdAsync(serviceId, cancellationToken))
        {
            throw new InvalidOperationException(ServiceManagementMessages.CannotDeleteWithActiveAppointments);
        }

        return await _serviceRepository.SoftDeleteAsync(serviceId, masterId, cancellationToken);
    }

    private async Task EnsureUniqueNameAsync(
        int masterId,
        string name,
        int? excludeServiceId,
        CancellationToken cancellationToken)
    {
        var existing = await _serviceRepository.GetByMasterIdAsync(masterId, cancellationToken);
        if (existing.Any(x =>
                x.Id != excludeServiceId &&
                x.ServiceName.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(ServiceManagementMessages.DuplicateServiceName);
        }
    }

    private static void ApplyRequest(Service service, ServiceUpsertRequestDTO request, string name)
    {
        service.ServiceName = name;
        service.DurationInMinutes = request.DurationInMinutes;
        service.Price = request.Price;
        service.Prepayment = request.Prepayment;
        service.PreparationBeforeInMinutes = request.PreparationBeforeInMinutes;
        service.PreparationAfterInMinutes = request.PreparationAfterInMinutes;
        service.TotalOccupiedMinutes = request.PreparationBeforeInMinutes
                                       + request.DurationInMinutes
                                       + request.PreparationAfterInMinutes;
    }

    private static ServiceResponseDTO MapToDto(Service service) =>
        new()
        {
            Id = service.Id,
            ServiceName = service.ServiceName,
            DurationInMinutes = service.DurationInMinutes,
            Price = service.Price,
            Prepayment = service.Prepayment,
            PreparationBeforeInMinutes = service.PreparationBeforeInMinutes,
            PreparationAfterInMinutes = service.PreparationAfterInMinutes,
            TotalOccupiedMinutes = service.TotalOccupiedMinutes,
            CreatedAt = service.CreatedAt,
            UpdatedAt = service.UpdatedAt
        };
}
