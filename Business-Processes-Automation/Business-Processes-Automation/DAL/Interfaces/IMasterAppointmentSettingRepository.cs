using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.BLL.Interfaces.Repositories;

public interface IMasterAppointmentSettingRepository
{
    Task<MasterAppointmentSetting?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MasterAppointmentSetting>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<MasterAppointmentSetting> CreateAsync(MasterAppointmentSetting entity, CancellationToken cancellationToken = default);
    Task<MasterAppointmentSetting> UpdateAsync(MasterAppointmentSetting entity, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
}
