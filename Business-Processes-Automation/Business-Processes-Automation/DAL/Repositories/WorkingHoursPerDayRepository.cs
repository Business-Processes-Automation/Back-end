using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.DAL.Repositories;

public class WorkingHoursPerDayRepository : IWorkingHoursPerDayRepository
{
    private readonly AppDbContext _dbContext;

    public WorkingHoursPerDayRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<WorkingHoursPerDay?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<WorkingHoursPerDay>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<WorkingHoursPerDay> CreateAsync(WorkingHoursPerDay entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<WorkingHoursPerDay> UpdateAsync(WorkingHoursPerDay entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
