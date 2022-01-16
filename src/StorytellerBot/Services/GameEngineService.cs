using Microsoft.Extensions.Options;
using StorytellerBot.Models;
using StorytellerBot.Settings;
using Story = Ink.Runtime.Story;

namespace StorytellerBot.Services;

public class GameEngineService
{
    private const string Extension = ".json";
    private readonly string _baseScriptsPath;

    public GameEngineService(
        IOptionsSnapshot<GameConfiguration> gameConfig, IHostEnvironment environment)
    {
        _baseScriptsPath = Path.Combine(environment.ContentRootPath, gameConfig.Value.ScriptsDirectory);
    }

    public IList<string> GetAvailableScripts() =>
        Directory.Exists(_baseScriptsPath)
            ? Directory
                .GetFiles(_baseScriptsPath, $"*{Extension}")
                .Select(Path.GetFileNameWithoutExtension)
                .ToList()!
            : new List<string>();

    public async Task<AdventureStep?> GetFirstMessage(string storyName)
    {
        var storyPath = Path.Combine(_baseScriptsPath, $"{storyName}{Extension}");
        if (!File.Exists(storyPath))
            return null;

        var jsonContent = await File.ReadAllTextAsync(storyPath);
        var story = new Story(jsonContent.Trim());

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
        };
    }
}
