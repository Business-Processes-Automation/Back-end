using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.DAL.Repositories;

public class MasterAppointmentSettingRepository : IMasterAppointmentSettingRepository
{
    private readonly AppDbContext _dbContext;

    public MasterAppointmentSettingRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<MasterAppointmentSetting?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<MasterAppointmentSetting>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<MasterAppointmentSetting> CreateAsync(MasterAppointmentSetting entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<MasterAppointmentSetting> UpdateAsync(MasterAppointmentSetting entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
