using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Business_Processes_Automation.DAL.Repositories;

public class MasterAppointmentSettingRepository : IMasterAppointmentSettingRepository
{
    private readonly AppDbContext _dbContext;

    public MasterAppointmentSettingRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<MasterAppointmentSetting?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.MasterAppointmentSettings
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<MasterAppointmentSetting>> GetAllAsync(
        CancellationToken cancellationToken = default) =>
        await _dbContext.MasterAppointmentSettings
            .OrderBy(x => x.MasterId)
            .ToListAsync(cancellationToken);

    public Task<MasterAppointmentSetting?> GetByMasterIdAsync(
        int masterId,
        CancellationToken cancellationToken = default) =>
        _dbContext.MasterAppointmentSettings
            .FirstOrDefaultAsync(x => x.MasterId == masterId, cancellationToken);

    public async Task<MasterAppointmentSetting> CreateAsync(
        MasterAppointmentSetting entity,
        CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        _dbContext.MasterAppointmentSettings.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<MasterAppointmentSetting> UpdateAsync(
        MasterAppointmentSetting entity,
        CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbContext.MasterAppointmentSettings.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.MasterAppointmentSettings
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
