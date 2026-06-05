using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.BLL.Results;

public sealed class MasterTelegramLinkResult
{
    public bool Success { get; init; }

    public Master? Master { get; init; }

    public MasterTelegram? MasterTelegram { get; init; }

    public string? ErrorMessage { get; init; }

    public static MasterTelegramLinkResult Ok(Master master, MasterTelegram masterTelegram) =>
        new() { Success = true, Master = master, MasterTelegram = masterTelegram };

    public static MasterTelegramLinkResult Fail(string message) =>
        new() { Success = false, ErrorMessage = message };
}
