using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.DAL.Repositories;

public class MasterRepository : IMasterRepository
{
    private readonly AppDbContext _dbContext;

    public MasterRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Master?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<Master>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Master> CreateAsync(Master entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Master> UpdateAsync(Master entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
