using Telegram.Bot;
using Telegram.Bot.Types;

namespace StorytellerBot.Services;

public class ConfigureCommands : IHostedService
{
    private readonly ILogger<ConfigureWebhook> _logger;
    private readonly IServiceProvider _services;

    public ConfigureCommands(
        ILogger<ConfigureWebhook> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _services = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        _logger.LogInformation("Setting commands");
        foreach (var (language, commands) in Commands.GetAll())
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            _logger.LogInformation("Setting commands for language {language}", language);
            await botClient.SetMyCommandsAsync(
                commands, BotCommandScope.AllPrivateChats(), language, cancellationToken);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        _logger.LogInformation("Removing commands");
        await botClient.DeleteMyCommandsAsync();
    }
}
