using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.BLL.SessionDrafts;
using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.BLL.Services;

public class ServiceManagementService : IServiceManagementService
{
    private readonly IServiceRepository _serviceRepository;

    public ServiceManagementService(IServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public Task<IReadOnlyList<Service>> GetByMasterIdAsync(
        int masterId,
        CancellationToken cancellationToken = default) =>
        _serviceRepository.GetByMasterIdAsync(masterId, cancellationToken);

    public async Task<ServiceCreateResult> CreateForMasterAsync(
        int masterId,
        MasterServiceDraft draft,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(draft.ServiceName))
        {
            return ServiceCreateResult.Fail("Вкажіть назву послуги.");
        }

        var name = draft.ServiceName.Trim();
        if (name.Length > 200)
        {
            return ServiceCreateResult.Fail("Назва занадто довга (максимум 200 символів).");
        }

        if (draft.DurationInMinutes is not (> 0 and <= 1440))
        {
            return ServiceCreateResult.Fail("Тривалість має бути від 1 до 1440 хвилин.");
        }

        if (draft.Price is not (>= 0 and <= 999999.99m))
        {
            return ServiceCreateResult.Fail("Ціна має бути від 0 до 999999.99 грн.");
        }

        var existing = await _serviceRepository.GetByMasterIdAsync(masterId, cancellationToken);
        if (existing.Any(x => x.ServiceName.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            return ServiceCreateResult.Fail("Послуга з такою назвою вже існує.");
        }

        var service = new Service
        {
            MasterId = masterId,
            ServiceName = name,
            DurationInMinutes = draft.DurationInMinutes.Value,
            Price = draft.Price.Value,
            Prepayment = 0
        };

        var created = await _serviceRepository.CreateAsync(service, cancellationToken);
        return ServiceCreateResult.Ok(created);
    }
}
