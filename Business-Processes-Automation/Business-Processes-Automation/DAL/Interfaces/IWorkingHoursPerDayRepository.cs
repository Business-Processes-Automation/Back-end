using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.Interfaces.Repositories;

public interface IWorkingHoursPerDayRepository
{
    Task<WorkingHoursPerDay?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkingHoursPerDay>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkingHoursPerDay>> GetByMasterIdAsync(
        int masterId,
        CancellationToken cancellationToken = default);

    Task<WorkingHoursPerDay?> GetByMasterAndDayAsync(
        int masterId,
        Weekday day,
        CancellationToken cancellationToken = default);

    Task<WorkingHoursPerDay> CreateAsync(WorkingHoursPerDay entity, CancellationToken cancellationToken = default);

    Task<WorkingHoursPerDay> UpdateAsync(WorkingHoursPerDay entity, CancellationToken cancellationToken = default);

    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
}
