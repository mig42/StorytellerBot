using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StorytellerBot.Data;
using StorytellerBot.Models;
using StorytellerBot.Settings;
using Story = Ink.Runtime.Story;

namespace StorytellerBot.Services;

public class GameEngineService
{
    private const string Extension = ".json";
    private readonly string _baseScriptsPath;
    private readonly AdventureContext _adventureContext;

    public GameEngineService(
        IOptionsSnapshot<GameConfiguration> gameConfig,
        IHostEnvironment environment,
        AdventureContext adventureContext)
    {
        _baseScriptsPath = Path.Combine(environment.ContentRootPath, gameConfig.Value.ScriptsDirectory);
        _adventureContext = adventureContext;
    }

    internal IList<Adventure> GetAdventures() =>
        _adventureContext.Adventures.AsNoTracking().OrderBy(a => a.Id).ToList();

    internal Adventure? GetAdventure(string name) =>
        _adventureContext.Adventures.AsNoTracking().FirstOrDefault(a => a.Name == name);

    internal SavedStatus? GetSavedStatus(Adventure adventure, long userId)
    {
        if (adventure == null)
            return null;
        return _adventureContext.SavedStatuses.AsNoTracking()
            .FirstOrDefault(s => s.UserId == userId && s.AdventureId == adventure.Id);
    }

    internal async Task<AdventureStep?> GetNextStep(Adventure? adventure, SavedStatus? savedStatus)
    {
        if (adventure == null)
            return null;

        var storyPath = Path.Combine(_baseScriptsPath, $"{adventure.ScriptFileName}{Extension}");
        if (!File.Exists(storyPath))
            return null;

        var jsonContent = await File.ReadAllTextAsync(storyPath);
        var story = new Story(jsonContent.Trim());
        if (savedStatus != null)
            story.state.LoadJson(savedStatus.StoryState);

        var paragraphs = new List<string>();
        while (story.canContinue)
        {
            story.Continue();
            paragraphs.Add(story.currentText);
        }

        return new AdventureStep
        {
            Paragraphs = paragraphs,
            Decisions = story.currentChoices.Select(choice => new Decision
            {
                Text = choice.text,
                Path = choice.pathStringOnChoice,
            }),
            IsEnding = !story.currentChoices.Any(),
            Story = story,
        };
    }

    internal User? GetUser(long id) =>
        _adventureContext.Users.AsNoTracking().FirstOrDefault(u => u.Id == id);
}
