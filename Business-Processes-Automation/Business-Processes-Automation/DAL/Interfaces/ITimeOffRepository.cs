using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.BLL.Interfaces.Repositories;

public interface ITimeOffRepository
{
    Task<TimeOff?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TimeOff>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TimeOff> CreateAsync(TimeOff entity, CancellationToken cancellationToken = default);
    Task<TimeOff> UpdateAsync(TimeOff entity, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
}
