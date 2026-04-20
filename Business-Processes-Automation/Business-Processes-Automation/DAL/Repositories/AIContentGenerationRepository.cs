using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.DAL.Repositories;

public class AIContentGenerationRepository : IAIContentGenerationRepository
{
    private readonly AppDbContext _dbContext;

    public AIContentGenerationRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AIContentGeneration?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<AIContentGeneration>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<AIContentGeneration> CreateAsync(AIContentGeneration entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<AIContentGeneration> UpdateAsync(AIContentGeneration entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
