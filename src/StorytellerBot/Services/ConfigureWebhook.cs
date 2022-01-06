using Microsoft.Extensions.Options;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;

using StorytellerBot.Settings;

namespace StorytellerBot.Services;

public class ConfigureWebhook : IHostedService
{
    private readonly ILogger<ConfigureWebhook> _logger;
    private readonly BotConfiguration _botConfig;
    private readonly IServiceProvider _services;

    public ConfigureWebhook(
        ILogger<ConfigureWebhook> logger,
        IOptions<BotConfiguration> botConfig,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _botConfig = botConfig.Value;
        _services = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        var webhookAddress = $"{_botConfig.HostAddress}/webhook/update/{_botConfig.WebhookToken}";
        _logger.LogInformation("Setting webhook: {webhookAddress}", webhookAddress);

        await botClient.SetWebhookAsync(
            url: webhookAddress,
            allowedUpdates: Array.Empty<UpdateType>(),
            cancellationToken: cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        _logger.LogInformation("Removing webhook");
        await botClient.DeleteWebhookAsync(cancellationToken: cancellationToken);
    }
}
