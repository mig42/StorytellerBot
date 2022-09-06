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
        await botClient.SetMyCommandsAsync(
            Commands.GetAll(), BotCommandScope.AllPrivateChats(), cancellationToken: cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        var commands = await botClient.GetMyCommandsAsync();
        foreach (var command in commands)
        {
            _logger.LogWarning("[POST] Detected command '{Command}': {Description}", command.Command, command.Description);
        }
        _logger.LogInformation("Removing commands");
        await botClient.DeleteMyCommandsAsync();
    }
}
