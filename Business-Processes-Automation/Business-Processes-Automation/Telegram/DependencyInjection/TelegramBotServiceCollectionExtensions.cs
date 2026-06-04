using Business_Processes_Automation.Telegram.Abstractions;
using Business_Processes_Automation.Telegram.Configuration;
using Business_Processes_Automation.Telegram.Handlers;
using Business_Processes_Automation.Telegram.Handlers.Commands;
using Business_Processes_Automation.Telegram.Handlers.Messages;
using Business_Processes_Automation.Telegram.Hosting;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace Business_Processes_Automation.Telegram.DependencyInjection;

public static class TelegramBotServiceCollectionExtensions
{
    public static IServiceCollection AddTelegramBot(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<TelegramBotOptions>()
            .Bind(configuration.GetSection(TelegramBotOptions.SectionName));

        services.AddSingleton<ITelegramBotClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<TelegramBotOptions>>().Value;
            return new TelegramBotClient(options.BotToken);
        });

        services.AddScoped<TelegramCommandDispatcher>();
        services.AddScoped<MessageUpdateHandler>();
        services.AddScoped<MasterRegistrationHandler>();
        services.AddScoped<MasterScheduleHandler>();
        services.AddScoped<MasterPanelHandler>();
        services.AddScoped<ClientBookingHandler>();
        services.AddScoped<MenuReplyHandler>();
        services.AddScoped<ITelegramUpdateHandler, TelegramUpdateHandler>();

        services.AddScoped<ITelegramCommandHandler, StartCommandHandler>();
        services.AddScoped<ITelegramCommandHandler, RegisterCommandHandler>();
        services.AddScoped<ITelegramCommandHandler, LogoutCommandHandler>();
        services.AddScoped<ITelegramCommandHandler, MenuCommandHandler>();
        services.AddScoped<ITelegramCommandHandler, CancelCommandHandler>();

        services.AddHostedService<TelegramBotHostedService>();

        return services;
    }
}
