using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;

using StorytellerBot.Services;
using StorytellerBot.Settings;

namespace StorytellerBot.Controllers;

[ApiController]
[Route("webhook")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
public class WebhookController : ControllerBase
{
    private readonly string _webhookToken;
    private readonly HandleUpdateService _handleUpdateService;

    public WebhookController(
        IOptionsSnapshot<BotConfiguration> botConfiguration,
        HandleUpdateService handleUpdateService)
    {
        _webhookToken = botConfiguration.Value?.WebhookToken ?? string.Empty;
        _handleUpdateService = handleUpdateService;
    }

    [HttpPost("update/{token}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(void))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(void))]
    public async Task<IActionResult> Update([FromRoute]string token, [FromBody] Update update)
    {
        if (_webhookToken != token)
            return Unauthorized();

        await _handleUpdateService.EchoAsync(update);
        return Ok();
    }
}
