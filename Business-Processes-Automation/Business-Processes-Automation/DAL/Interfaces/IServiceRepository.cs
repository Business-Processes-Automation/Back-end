using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.BLL.Interfaces.Repositories;

public interface IServiceRepository
{
    Task<IReadOnlyList<Service>> GetByMasterIdAsync(int masterId, CancellationToken cancellationToken = default);

    Task<Service?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Service>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Service> CreateAsync(Service entity, CancellationToken cancellationToken = default);
    Task<Service> UpdateAsync(Service entity, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
}
