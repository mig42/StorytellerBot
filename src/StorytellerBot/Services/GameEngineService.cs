using Microsoft.Extensions.Options;

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

    public string? GetFirstMessage(string storyName)
    {
        string storyPath = Path.Combine(_baseScriptsPath, $"{storyName}{Extension}");
        if (!File.Exists(storyPath))
            return null;

        var story = new Story(storyPath);
        return story.currentText;
    }
}
