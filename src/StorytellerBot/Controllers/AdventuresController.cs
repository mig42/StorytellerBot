using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using StorytellerBot.Settings;
using StorytellerBot.Data;
using StorytellerBot.Models.Input;
using StorytellerBot.Models.Data;

namespace StorytellerBot.Controllers;

[ApiController]
[Route("adventures")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
public class AdventuresController : ControllerBase
{
    private readonly string _webhookToken;
    private readonly AdventureRepository _repo;

    public AdventuresController(
        IOptionsSnapshot<BotConfiguration> botConfiguration,
        AdventureRepository repo)
    {
        _webhookToken = botConfiguration.Value?.WebhookToken ?? string.Empty;
        _repo = repo;
    }

    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdventureInputModel))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(void))]
    public async Task<ActionResult<Adventure>> Create(
        [FromHeader(Name = TokenHeader)]string token, [FromBody] AdventureInputModel model)
    {
        if (_webhookToken != token)
            return Unauthorized();

        var newAdventure = new Adventure
        {
            Name = model.Name,
            Description = model.Description,
            ScriptFileName = model.ScriptFileName,
        };
        return await _repo.AddAdventureAsync(newAdventure);
    }

    [HttpGet("")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Adventure[]))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(void))]
    public async Task<ActionResult<List<Adventure>>> GetAll([FromHeader(Name = TokenHeader)]string token)
    {
        if (_webhookToken != token)
            return Unauthorized();

        return await _repo.GetAllAdventuresAsync();
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Adventure))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(void))]
    public async Task<ActionResult<Adventure>> Get(
        [FromHeader(Name = TokenHeader)]string token, [FromRoute] int id)
    {
        if (_webhookToken != token)
            return Unauthorized();

        var adventure = await _repo.GetAdventureAsync(id);
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

        return await _repo.DeleteAdventureAsync(id) ? Ok() : NotFound();
    }

    [HttpDelete("{id:int}/saves")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(void))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(void))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(void))]
    public async Task<IActionResult> DeleteSavesForAdventure(
        [FromHeader(Name = TokenHeader)]string token, [FromRoute] int id)
    {
        if (_webhookToken != token)
            return Unauthorized();

        return await _repo.DeleteSavesForAdventureAsync(id) ? Ok() : NotFound();
    }

    const string TokenHeader = "Webhook-Token";
}
