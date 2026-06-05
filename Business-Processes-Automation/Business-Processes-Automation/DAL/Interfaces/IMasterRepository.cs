using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.BLL.Interfaces.Repositories;

public interface IMasterRepository
{
    Task<Master?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Master?> GetByIdWithTelegramAsync(int id, CancellationToken cancellationToken = default);

    Task<Master?> GetByTelegramLinkCodeAsync(string linkCode, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Master>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Master> CreateAsync(Master entity, CancellationToken cancellationToken = default);
    Task<Master> UpdateAsync(Master entity, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
}
