using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.DAL.Entities;
using Business_Processes_Automation.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace Business_Processes_Automation.DAL.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private static readonly AppointmentStatus[] DefaultActiveStatuses =
    [
        AppointmentStatus.Planned,
        AppointmentStatus.Rescheduled
    ];

    private readonly AppDbContext _dbContext;

    public AppointmentRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Appointment?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.Appointments
            .Include(x => x.Service)
            .Include(x => x.Client)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Appointment>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Appointments
            .Include(x => x.Service)
            .Include(x => x.Client)
            .OrderBy(x => x.StartDateTime)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Appointment>> GetByMasterIdInRangeAsync(
        int masterId,
        DateTime fromUtc,
        DateTime toUtc,
        IReadOnlyCollection<AppointmentStatus>? statuses = null,
        int? serviceId = null,
        CancellationToken cancellationToken = default)
    {
        var statusFilter = statuses ?? DefaultActiveStatuses;

        var query = _dbContext.Appointments
            .Include(x => x.Service)
            .Include(x => x.Client)
            .Where(x => x.Service.MasterId == masterId
                        && !x.IsDeleted
                        && x.StartDateTime < toUtc
                        && x.EndDateTime > fromUtc
                        && statusFilter.Contains(x.Status));

        if (serviceId is not null)
        {
            query = query.Where(x => x.ServiceId == serviceId.Value);
        }

        return await query
            .OrderBy(x => x.StartDateTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Appointment>> GetByClientTelegramIdForMasterAsync(
        long telegramUserId,
        int masterId,
        CancellationToken cancellationToken = default) =>
        await _dbContext.Appointments
            .Include(x => x.Service)
            .Include(x => x.Client)
            .Where(x => !x.IsDeleted
                        && !x.Client.IsDeleted
                        && x.Client.ClientTelegramId == telegramUserId
                        && x.Service.MasterId == masterId)
            .OrderBy(x => x.StartDateTime)
            .ToListAsync(cancellationToken);

    public Task<Appointment?> GetByIdForMasterAsync(
        int appointmentId,
        int masterId,
        CancellationToken cancellationToken = default) =>
        _dbContext.Appointments
            .Include(x => x.Service)
            .Include(x => x.Client)
            .FirstOrDefaultAsync(
                x => x.Id == appointmentId && x.Service.MasterId == masterId && !x.IsDeleted,
                cancellationToken);

    public async Task<Appointment> CreateAsync(Appointment entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        _dbContext.Appointments.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<Appointment> UpdateAsync(Appointment entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbContext.Appointments.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Appointments
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

    public Task<bool> HasActiveByServiceIdAsync(
        int serviceId,
        CancellationToken cancellationToken = default) =>
        _dbContext.Appointments.AnyAsync(
            x => x.ServiceId == serviceId
                 && !x.IsDeleted
                 && DefaultActiveStatuses.Contains(x.Status),
            cancellationToken);
}
