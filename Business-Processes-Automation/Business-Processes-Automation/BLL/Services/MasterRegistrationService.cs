using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.BLL.SessionDrafts;
using Business_Processes_Automation.DAL.Entities;

namespace Business_Processes_Automation.BLL.Services;

public class MasterRegistrationService : IMasterRegistrationService
{
    private static readonly Regex BotStartParameterRegex = new("^[a-z0-9_]{3,32}$", RegexOptions.Compiled);

    private readonly IMasterTelegramRepository _masterTelegramRepository;
    private readonly IMasterRepository _masterRepository;
    private readonly IMasterAppointmentSettingRepository _appointmentSettingRepository;

    public MasterRegistrationService(
        IMasterTelegramRepository masterTelegramRepository,
        IMasterRepository masterRepository,
        IMasterAppointmentSettingRepository appointmentSettingRepository)
    {
        _masterTelegramRepository = masterTelegramRepository;
        _masterRepository = masterRepository;
        _appointmentSettingRepository = appointmentSettingRepository;
    }

    public Task<MasterTelegram?> GetByTelegramUserIdAsync(
        long telegramUserId,
        CancellationToken cancellationToken = default) =>
        _masterTelegramRepository.GetByTelegramUserIdAsync(telegramUserId, cancellationToken);

    public async Task<bool> IsBotStartParameterTakenAsync(
        string botStartParameter,
        CancellationToken cancellationToken = default)
    {
        var existing = await _masterTelegramRepository.GetByBotStartParameterAsync(
            botStartParameter,
            cancellationToken);

        return existing is not null;
    }

    public async Task<MasterRegistrationResult> RegisterAsync(
        long telegramUserId,
        MasterRegistrationDraft draft,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateDraft(draft);
        if (validationError is not null)
        {
            return MasterRegistrationResult.Fail(validationError);
        }

        var existingByUser = await _masterTelegramRepository.GetByTelegramUserIdAsync(
            telegramUserId,
            cancellationToken);
        if (existingByUser is not null)
        {
            return MasterRegistrationResult.Fail("Ви вже зареєстровані як майстер.");
        }

        var botStartParameter = draft.BotStartParameter!.Trim().ToLowerInvariant();
        if (await IsBotStartParameterTakenAsync(botStartParameter, cancellationToken))
        {
            return MasterRegistrationResult.Fail("Це посилання вже зайняте. Оберіть інший ідентифікатор.");
        }

        var master = new Master
        {
            FirstName = draft.FirstName!.Trim(),
            LastName = draft.LastName!.Trim(),
            Username = botStartParameter,
            Email = draft.Email!.Trim(),
            PhoneNumber = draft.PhoneNumber!.Trim(),
            TimeZone = draft.TimeZone!.Trim(),
            IsActive = true
        };

        master = await _masterRepository.CreateAsync(master, cancellationToken);

        await _appointmentSettingRepository.CreateAsync(
            new MasterAppointmentSetting
            {
                MasterId = master.Id,
                MinBookingNoticeMinutes = 60,
                MaxBookingDaysAhead = 30,
                CancellationPolicyHours = 24,
                BufferBetweenClientsMinutes = 10,
                MaxRescheduleCount = 1
            },
            cancellationToken);

        var telegramUsername = string.IsNullOrWhiteSpace(draft.TelegramUsername)
            ? botStartParameter
            : draft.TelegramUsername.Trim().TrimStart('@');

        var masterTelegram = await _masterTelegramRepository.CreateAsync(
            new MasterTelegram
            {
                MasterId = master.Id,
                TelegramUserId = telegramUserId,
                TelegramUsername = telegramUsername,
                BotStartParameter = botStartParameter
            },
            cancellationToken);

        return MasterRegistrationResult.Ok(master, masterTelegram);
    }

    private static string? ValidateDraft(MasterRegistrationDraft draft)
    {
        if (string.IsNullOrWhiteSpace(draft.FirstName) || draft.FirstName.Trim().Length > 100)
        {
            return "Вкажіть ім'я (до 100 символів).";
        }

        if (string.IsNullOrWhiteSpace(draft.LastName) || draft.LastName.Trim().Length > 100)
        {
            return "Вкажіть прізвище (до 100 символів).";
        }

        if (string.IsNullOrWhiteSpace(draft.PhoneNumber) || draft.PhoneNumber.Trim().Length > 20)
        {
            return "Вкажіть номер телефону (до 20 символів).";
        }

        if (string.IsNullOrWhiteSpace(draft.Email) ||
            !new EmailAddressAttribute().IsValid(draft.Email.Trim()))
        {
            return "Вкажіть коректний email.";
        }

        if (string.IsNullOrWhiteSpace(draft.TimeZone) || draft.TimeZone.Trim().Length > 64)
        {
            return "Вкажіть часовий пояс (наприклад, Europe/Kyiv).";
        }

        if (string.IsNullOrWhiteSpace(draft.BotStartParameter))
        {
            return "Вкажіть ідентифікатор для посилання.";
        }

        var slug = draft.BotStartParameter.Trim().ToLowerInvariant();
        if (!BotStartParameterRegex.IsMatch(slug))
        {
            return "Ідентифікатор: лише латинські літери, цифри та _, від 3 до 32 символів.";
        }

        return null;
    }
}
