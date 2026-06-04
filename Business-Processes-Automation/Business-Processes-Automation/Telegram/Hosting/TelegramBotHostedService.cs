using Business_Processes_Automation.Telegram.Abstractions;
using Business_Processes_Automation.Telegram.Configuration;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Business_Processes_Automation.Telegram.Hosting;

public class TelegramBotHostedService : BackgroundService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TelegramBotOptions _options;
    private readonly ILogger<TelegramBotHostedService> _logger;

    public TelegramBotHostedService(
        ITelegramBotClient botClient,
        IServiceScopeFactory scopeFactory,
        IOptions<TelegramBotOptions> options,
        ILogger<TelegramBotHostedService> logger)
    {
        _botClient = botClient;
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.UsePolling)
        {
            _logger.LogInformation("Telegram polling is disabled (UsePolling = false).");
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.BotToken))
        {
            _logger.LogWarning(
                "Telegram bot token is not configured. Set Telegram:BotToken in appsettings.Development.local.json or User Secrets.");
            return;
        }

        var me = await _botClient.GetMe(stoppingToken);
        _logger.LogInformation(
            "Telegram bot started: @{BotUsername} (id {BotId}). Long polling is active.",
            me.Username,
            me.Id);

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = [UpdateType.Message]
        };

        _botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            errorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Telegram bot polling stopped.");
        }
    }

    private async Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ITelegramUpdateHandler>();

        try
        {
            await handler.HandleUpdateAsync(botClient, update, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in Telegram update pipeline for update {UpdateId}", update.Id);
        }
    }

    private Task HandlePollingErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Telegram polling error");
        return Task.CompletedTask;
    }
}
