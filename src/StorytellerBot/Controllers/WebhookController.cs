using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;

using StorytellerBot.Services;
using StorytellerBot.Settings;
using Telegram.Bot.Exceptions;

namespace StorytellerBot.Controllers;

[ApiController]
[Route("webhook")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
public class WebhookController : ControllerBase
{
    private readonly string _webhookToken;
    private readonly IConversationFactory _messageGeneratorFactory;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(
        IOptionsSnapshot<BotConfiguration> botConfiguration,
        IConversationFactory messageGeneratorFactory,
        ILogger<WebhookController> logger)
    {
        _webhookToken = botConfiguration.Value?.WebhookToken ?? string.Empty;
        _messageGeneratorFactory = messageGeneratorFactory;
        _logger = logger;
    }

    [HttpPost("update/{token}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(void))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(void))]
    public async Task<IActionResult> Update([FromRoute]string token, [FromBody] Update update)
    {
        if (_webhookToken != token)
            return Unauthorized();

        var messageGenerator = _messageGeneratorFactory.Create(update);
        if (messageGenerator == null)
        {
            _logger.LogInformation(
                "Unsupported update type '{UpdateType}' from user #{UserId}", update.Type, update.Message?.From?.Id);
            return Ok();
        }
        try {
            await messageGenerator.SendResponsesAsync(update);
        } catch (Exception e) {
            var errorMessage = e switch
            {
                ApiRequestException apiRequestException =>
                    $"Telegram API Error: [{apiRequestException.ErrorCode}] {apiRequestException.Message}",
                _ => e.Message,
            };

            _logger.LogError(e, "Error sending messages: {ErrorMessage}", errorMessage);
        }
        return Ok();
    }
}
