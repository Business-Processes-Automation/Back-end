using Business_Processes_Automation.BLL.SessionDrafts;
using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.BLL.Services;

public interface IServiceManagementService
{
    Task<IReadOnlyList<Service>> GetByMasterIdAsync(int masterId, CancellationToken cancellationToken = default);

    Task<ServiceCreateResult> CreateForMasterAsync(
        int masterId,
        MasterServiceDraft draft,
        CancellationToken cancellationToken = default);
}

public sealed class ServiceCreateResult
{
    public bool Success { get; init; }

    public Service? Service { get; init; }

    public string? ErrorMessage { get; init; }

    public static ServiceCreateResult Ok(Service service) =>
        new() { Success = true, Service = service };

    public static ServiceCreateResult Fail(string message) =>
        new() { Success = false, ErrorMessage = message };
}
