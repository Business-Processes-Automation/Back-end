using Business_Processes_Automation.BLL.DTOs.Service;
using Business_Processes_Automation.BLL.Interfaces;
using Business_Processes_Automation.BLL.Interfaces.Repositories;
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
        var name = EnsureValidRequest(
            request.ServiceName,
            request.DurationInMinutes,
            request.Price,
            request.Prepayment,
            request.PreparationBeforeInMinutes,
            request.PreparationAfterInMinutes);

        if (await IsDuplicateNameAsync(masterId, name, excludeServiceId: null, cancellationToken))
        {
            throw new InvalidOperationException("Послуга з такою назвою вже існує.");
        }

        var service = new Service
        {
            MasterId = masterId,
            ServiceName = name,
            DurationInMinutes = request.DurationInMinutes,
            Price = request.Price,
            Prepayment = request.Prepayment,
            PreparationBeforeInMinutes = request.PreparationBeforeInMinutes,
            PreparationAfterInMinutes = request.PreparationAfterInMinutes
        };

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

        var name = EnsureValidRequest(
            request.ServiceName,
            request.DurationInMinutes,
            request.Price,
            request.Prepayment,
            request.PreparationBeforeInMinutes,
            request.PreparationAfterInMinutes);

        if (await IsDuplicateNameAsync(masterId, name, serviceId, cancellationToken))
        {
            throw new InvalidOperationException("Послуга з такою назвою вже існує.");
        }

        service.ServiceName = name;
        service.DurationInMinutes = request.DurationInMinutes;
        service.Price = request.Price;
        service.Prepayment = request.Prepayment;
        service.PreparationBeforeInMinutes = request.PreparationBeforeInMinutes;
        service.PreparationAfterInMinutes = request.PreparationAfterInMinutes;

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
            throw new InvalidOperationException("Неможливо видалити послугу: є активні записи.");
        }

        return await _serviceRepository.SoftDeleteAsync(serviceId, masterId, cancellationToken);
    }

    private async Task<bool> IsDuplicateNameAsync(
        int masterId,
        string name,
        int? excludeServiceId,
        CancellationToken cancellationToken)
    {
        var existing = await _serviceRepository.GetByMasterIdAsync(masterId, cancellationToken);
        return existing.Any(x =>
            x.Id != excludeServiceId &&
            x.ServiceName.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    private static string EnsureValidRequest(
        string serviceName,
        int durationInMinutes,
        decimal price,
        decimal prepayment,
        int preparationBeforeInMinutes,
        int preparationAfterInMinutes)
    {
        var name = serviceName?.Trim() ?? string.Empty;
        if (name.Length is < 2 or > 200)
        {
            throw new InvalidOperationException("Назва має містити від 2 до 200 символів.");
        }

        if (durationInMinutes is < 1 or > 1440)
        {
            throw new InvalidOperationException("Тривалість має бути від 1 до 1440 хвилин.");
        }

        if (price is < 0 or > 999999.99m)
        {
            throw new InvalidOperationException("Ціна має бути від 0 до 999999.99 грн.");
        }

        if (prepayment is < 0 or > 999999.99m)
        {
            throw new InvalidOperationException("Передоплата має бути від 0 до 999999.99 грн.");
        }

        if (prepayment > price)
        {
            throw new InvalidOperationException("Передоплата не може перевищувати ціну послуги.");
        }

        if (preparationBeforeInMinutes is < 0 or > 480)
        {
            throw new InvalidOperationException("Підготовка до має бути від 0 до 480 хвилин.");
        }

        if (preparationAfterInMinutes is < 0 or > 480)
        {
            throw new InvalidOperationException("Підготовка після має бути від 0 до 480 хвилин.");
        }

        return name;
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
            CreatedAt = service.CreatedAt,
            UpdatedAt = service.UpdatedAt
        };
}
