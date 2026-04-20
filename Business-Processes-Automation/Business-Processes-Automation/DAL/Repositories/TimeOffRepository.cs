using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.DAL.Repositories;

public class TimeOffRepository : ITimeOffRepository
{
    private readonly AppDbContext _dbContext;

    public TimeOffRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<TimeOff?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<TimeOff>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<TimeOff> CreateAsync(TimeOff entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<TimeOff> UpdateAsync(TimeOff entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
