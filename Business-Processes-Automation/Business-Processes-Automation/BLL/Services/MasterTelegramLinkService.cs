using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Business_Processes_Automation.BLL.DTOs.Master;
using Business_Processes_Automation.BLL.Interfaces.Repositories;
using Business_Processes_Automation.BLL.Localization;
using Business_Processes_Automation.BLL.Results;
using Business_Processes_Automation.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Business_Processes_Automation.BLL.Services;

public class MasterTelegramLinkService : IMasterTelegramLinkService
{
    private const int LinkCodeLength = 8;
    private static readonly TimeSpan LinkCodeLifetime = TimeSpan.FromMinutes(15);
    private static readonly Regex BotStartParameterRegex = new("^[a-z0-9_]{3,32}$", RegexOptions.Compiled);
    private static readonly Regex LinkCodeInputRegex = new("^[A-Za-z0-9]{8}$", RegexOptions.Compiled);
    private const string LinkCodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    private readonly IMasterTelegramRepository _masterTelegramRepository;
    private readonly IMasterRepository _masterRepository;
    private readonly IMasterAppointmentSettingRepository _appointmentSettingRepository;

    public MasterTelegramLinkService(
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

    private async Task<bool> IsBotStartParameterTakenByAnotherMasterAsync(
        string botStartParameter,
        int masterId,
        CancellationToken cancellationToken)
    {
        var existing = await _masterTelegramRepository.GetByBotStartParameterIncludingDeletedAsync(
            botStartParameter,
            cancellationToken);

        return existing is { IsDeleted: false } && existing.MasterId != masterId;
    }

    public async Task<TelegramLinkCodeResponseDTO> GenerateLinkCodeAsync(
        int masterId,
        CancellationToken cancellationToken = default)
    {
        var master = await _masterRepository.GetByIdWithTelegramAsync(masterId, cancellationToken)
            ?? throw new InvalidOperationException(AuthMessages.MasterAccountNotFound);

        if (master.MasterTelegram is not null)
        {
            throw new InvalidOperationException(MasterTelegramLinkMessages.TelegramAlreadyLinkedToWebAccount);
        }

        master.TelegramLinkCode = GenerateLinkCode();
        master.TelegramLinkCodeExpiresAtUtc = DateTime.UtcNow.Add(LinkCodeLifetime);
        await _masterRepository.UpdateAsync(master, cancellationToken);

        return new TelegramLinkCodeResponseDTO
        {
            Code = master.TelegramLinkCode,
            ExpiresAtUtc = master.TelegramLinkCodeExpiresAtUtc.Value
        };
    }

    public async Task<TelegramLinkStatusResponseDTO> GetTelegramLinkStatusAsync(
        int masterId,
        CancellationToken cancellationToken = default)
    {
        var master = await _masterRepository.GetByIdWithTelegramAsync(masterId, cancellationToken)
            ?? throw new InvalidOperationException(AuthMessages.MasterAccountNotFound);

        return new TelegramLinkStatusResponseDTO
        {
            IsLinked = master.MasterTelegram is not null,
            BotStartParameter = master.MasterTelegram?.BotStartParameter
        };
    }

    public async Task<string?> ValidateLinkCodeAsync(
        string linkCode,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = NormalizeLinkCode(linkCode);
        if (!LinkCodeInputRegex.IsMatch(normalizedCode))
        {
            return MasterTelegramLinkMessages.InvalidLinkCodeFormat;
        }

        var master = await FindMasterByLinkCodeAsync(normalizedCode, cancellationToken);
        if (master is null)
        {
            return MasterTelegramLinkMessages.InvalidOrExpiredLinkCode;
        }

        if (master.MasterTelegram is not null)
        {
            return MasterTelegramLinkMessages.AccountAlreadyLinked;
        }

        return null;
    }

    public async Task<MasterTelegramLinkResult> LinkTelegramAsync(
        string linkCode,
        long telegramUserId,
        string telegramUsername,
        string botStartParameter,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = NormalizeLinkCode(linkCode);
        var validationError = await ValidateLinkCodeAsync(normalizedCode, cancellationToken);
        if (validationError is not null)
        {
            return MasterTelegramLinkResult.Fail(validationError);
        }

        var slug = botStartParameter.Trim().ToLowerInvariant();
        if (!BotStartParameterRegex.IsMatch(slug))
        {
            return MasterTelegramLinkResult.Fail(MasterTelegramLinkMessages.InvalidBotLinkFormat);
        }

        var master = await FindMasterByLinkCodeAsync(normalizedCode, cancellationToken);
        if (master is null)
        {
            return MasterTelegramLinkResult.Fail(MasterTelegramLinkMessages.InvalidOrExpiredLinkCode);
        }

        if (await IsBotStartParameterTakenByAnotherMasterAsync(slug, master.Id, cancellationToken))
        {
            return MasterTelegramLinkResult.Fail(MasterTelegramLinkMessages.BotLinkTaken);
        }

        await EnsureAppointmentSettingsAsync(master, cancellationToken);

        var username = string.IsNullOrWhiteSpace(telegramUsername)
            ? slug
            : telegramUsername.Trim().TrimStart('@');

        var existingByUser = await _masterTelegramRepository.GetByTelegramUserIdIncludingDeletedAsync(
            telegramUserId,
            cancellationToken);

        MasterTelegram masterTelegram;
        try
        {
            var upsertResult = await UpsertMasterTelegramAsync(
                existingByUser,
                master,
                telegramUserId,
                username,
                slug,
                cancellationToken);

            if (upsertResult.ErrorMessage is not null)
            {
                return MasterTelegramLinkResult.Fail(upsertResult.ErrorMessage);
            }

            masterTelegram = upsertResult.MasterTelegram!;
        }
        catch (DbUpdateException)
        {
            return MasterTelegramLinkResult.Fail(MasterTelegramLinkMessages.TelegramAlreadyLinkedToOtherAccount);
        }

        master.TelegramLinkCode = null;
        master.TelegramLinkCodeExpiresAtUtc = null;
        await _masterRepository.UpdateAsync(master, cancellationToken);

        return MasterTelegramLinkResult.Ok(master, masterTelegram);
    }

    private async Task<(MasterTelegram? MasterTelegram, string? ErrorMessage)> UpsertMasterTelegramAsync(
        MasterTelegram? existingByUser,
        Master master,
        long telegramUserId,
        string telegramUsername,
        string botStartParameter,
        CancellationToken cancellationToken)
    {
        if (existingByUser is { IsDeleted: false })
        {
            if (existingByUser.MasterId == master.Id)
            {
                existingByUser.TelegramUsername = telegramUsername;
                existingByUser.BotStartParameter = botStartParameter;
                var updated = await _masterTelegramRepository.UpdateAsync(existingByUser, cancellationToken);
                return (updated, null);
            }

            var linkedMaster = await _masterRepository.GetByIdAsync(existingByUser.MasterId, cancellationToken);
            if (linkedMaster is not null)
            {
                return (null, MasterTelegramLinkMessages.TelegramAlreadyLinkedToOtherAccount);
            }

            existingByUser.MasterId = master.Id;
            existingByUser.TelegramUsername = telegramUsername;
            existingByUser.BotStartParameter = botStartParameter;
            var reassigned = await _masterTelegramRepository.UpdateAsync(existingByUser, cancellationToken);
            return (reassigned, null);
        }

        if (existingByUser is { IsDeleted: true })
        {
            existingByUser.IsDeleted = false;
            existingByUser.MasterId = master.Id;
            existingByUser.TelegramUserId = telegramUserId;
            existingByUser.TelegramUsername = telegramUsername;
            existingByUser.BotStartParameter = botStartParameter;
            var restored = await _masterTelegramRepository.UpdateAsync(existingByUser, cancellationToken);
            return (restored, null);
        }

        var created = await _masterTelegramRepository.CreateAsync(
            new MasterTelegram
            {
                MasterId = master.Id,
                TelegramUserId = telegramUserId,
                TelegramUsername = telegramUsername,
                BotStartParameter = botStartParameter
            },
            cancellationToken);

        return (created, null);
    }

    private async Task<Master?> FindMasterByLinkCodeAsync(
        string normalizedCode,
        CancellationToken cancellationToken)
    {
        var master = await _masterRepository.GetByTelegramLinkCodeAsync(normalizedCode, cancellationToken);
        if (master is null)
        {
            return null;
        }

        if (master.TelegramLinkCodeExpiresAtUtc is null ||
            master.TelegramLinkCodeExpiresAtUtc <= DateTime.UtcNow)
        {
            return null;
        }

        return master;
    }

    private async Task EnsureAppointmentSettingsAsync(Master master, CancellationToken cancellationToken)
    {
        if (master.AppointmentSetting is not null)
        {
            return;
        }

        await _appointmentSettingRepository.CreateAsync(
            new MasterAppointmentSetting
            {
                MasterId = master.Id,
                MinBookingNoticeMinutes = 60,
                MaxBookingDaysAhead = 30,
                CancellationPolicyHours = 24,
                BufferBetweenClientsMinutes = 10,
                FreeSlotIntervalMinutes = 15,
                MaxRescheduleCount = 2
            },
            cancellationToken);
    }

    private static string NormalizeLinkCode(string linkCode) =>
        linkCode.Trim().ToUpperInvariant();

    private static string GenerateLinkCode()
    {
        Span<char> code = stackalloc char[LinkCodeLength];
        Span<byte> randomBytes = stackalloc byte[LinkCodeLength];

        RandomNumberGenerator.Fill(randomBytes);
        for (var i = 0; i < LinkCodeLength; i++)
        {
            code[i] = LinkCodeAlphabet[randomBytes[i] % LinkCodeAlphabet.Length];
        }

        return new string(code);
    }
}
