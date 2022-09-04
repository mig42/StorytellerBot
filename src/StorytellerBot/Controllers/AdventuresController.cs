using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using StorytellerBot.Settings;
using StorytellerBot.Data;
using StorytellerBot.Models;

namespace StorytellerBot.Controllers;

[ApiController]
[Route("adventures")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
public class AdventuresController : ControllerBase
{
    private readonly string _webhookToken;
    private readonly AdventureContext _adventureContext;

    public AdventuresController(
        IOptionsSnapshot<BotConfiguration> botConfiguration,
        AdventureContext adventureContext)
    {
        _webhookToken = botConfiguration.Value?.WebhookToken ?? string.Empty;
        _adventureContext = adventureContext;
    }

    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Adventure))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(void))]
    public async Task<ActionResult<Adventure>> Create(
        [FromHeader(Name = TokenHeader)]string token, [FromBody] Adventure newAdventure)
    {
        if (_webhookToken != token)
            return Unauthorized();

        await _adventureContext.Adventures.AddAsync(newAdventure);
        await _adventureContext.SaveChangesAsync();
        return newAdventure;
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Adventure))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(void))]
    public ActionResult<Adventure> Get(
        [FromHeader(Name = TokenHeader)]string token, [FromRoute] int id)
    {
        if (_webhookToken != token)
            return Unauthorized();

        var adventure = GetAdventure(id);
        return adventure == null ? NotFound() : adventure;
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Adventure))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(void))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(void))]
    public async Task<IActionResult> Delete(
        [FromHeader(Name = TokenHeader)]string token, [FromRoute] int id)
    {
        if (_webhookToken != token)
            return Unauthorized();

        var adventure = GetAdventure(id);
        if (adventure == null)
            return NotFound();
        _adventureContext.Adventures.Remove(adventure);
        await _adventureContext.SaveChangesAsync();
        return Ok();
    }

    private Adventure? GetAdventure(int id) =>
        _adventureContext.Adventures.AsNoTracking().SingleOrDefault(a => a.Id == id);

    const string TokenHeader = "Webhook-Token";
}
