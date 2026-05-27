using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;

namespace Business_Processes_Automation.BLL.Interfaces.Repositories;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Appointment>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Appointment>> GetByMasterIdInRangeAsync(
        int masterId,
        DateTime fromUtc,
        DateTime toUtc,
        IReadOnlyCollection<AppointmentStatus>? statuses = null,
        CancellationToken cancellationToken = default);

    Task<Appointment?> GetByIdForMasterAsync(
        int appointmentId,
        int masterId,
        CancellationToken cancellationToken = default);

    Task<Appointment> CreateAsync(Appointment entity, CancellationToken cancellationToken = default);

    Task<Appointment> UpdateAsync(Appointment entity, CancellationToken cancellationToken = default);

    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
}
