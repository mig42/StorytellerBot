using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;

using StorytellerBot.Services;
using StorytellerBot.Settings;
using Telegram.Bot.Exceptions;
using StorytellerBot.Models.Game;

namespace StorytellerBot.Controllers;

[ApiController]
[Route("webhook")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
public class WebhookController : ControllerBase
{
    private readonly string _webhookToken;
    private readonly IConversationFactory _messageGeneratorFactory;
    private readonly IResponseSender _responseSender;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(
        IOptionsSnapshot<BotConfiguration> botConfiguration,
        IResponseSender responseSender,
        IConversationFactory messageGeneratorFactory,
        ILogger<WebhookController> logger)
    {
        _webhookToken = botConfiguration.Value?.WebhookToken ?? string.Empty;
        _responseSender = responseSender;
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

        IEnumerable<Response> responses;
        try {
            responses = await messageGenerator.GetResponsesAsync(update);
        } catch (Exception e) {
            _logger.LogError(e, "Error building responses: {ErrorMessage}", e.Message);
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }

        Forget(SendResponses(responses));
        return Ok();
    }

    private void Forget(Task task)
    {
        _ = task.ConfigureAwait(false);
    }

    private async Task SendResponses(IEnumerable<Response> responses)
    {
        try
        {
            var sentMessages = await _responseSender.SendResponsesAsync(responses);
            _logger.LogInformation("Sent {SentMessagesCount} messages", sentMessages.Count);
        }
        catch (Exception e)
        {
            var errorMessage = e switch
            {
                ApiRequestException apiRequestException =>
                    $"Telegram API Error: [{apiRequestException.ErrorCode}] {apiRequestException.Message}",
                _ => e.Message,
            };

            _logger.LogError(e, "Error sending messages: {ErrorMessage}", errorMessage);
        }
    }
}
