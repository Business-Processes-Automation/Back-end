using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.BLL.Interfaces.Repositories;

public interface IPlatformRepository
{
    Task<Platform?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Platform>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Platform> CreateAsync(Platform entity, CancellationToken cancellationToken = default);
    Task<Platform> UpdateAsync(Platform entity, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
}
