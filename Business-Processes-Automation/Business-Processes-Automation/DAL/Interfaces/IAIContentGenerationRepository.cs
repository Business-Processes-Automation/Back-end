using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.BLL.Interfaces.Repositories;

public interface IAIContentGenerationRepository
{
    Task<AIContentGeneration?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AIContentGeneration>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AIContentGeneration> CreateAsync(AIContentGeneration entity, CancellationToken cancellationToken = default);
    Task<AIContentGeneration> UpdateAsync(AIContentGeneration entity, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
}
