using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.BLL.Interfaces.Repositories;

public interface IMasterTelegramRepository
{
    Task<MasterTelegram?> GetByBotStartParameterAsync(string botStartParameter, CancellationToken cancellationToken = default);

    Task<MasterTelegram?> GetByTelegramUserIdAsync(long telegramUserId, CancellationToken cancellationToken = default);

    Task<MasterTelegram?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MasterTelegram>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<MasterTelegram> CreateAsync(MasterTelegram entity, CancellationToken cancellationToken = default);
    Task<MasterTelegram> UpdateAsync(MasterTelegram entity, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
}
