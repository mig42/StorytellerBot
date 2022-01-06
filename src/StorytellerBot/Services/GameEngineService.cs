using Microsoft.Extensions.Options;
using StorytellerBot.Settings;

namespace StorytellerBot.Services;

public class GameEngineService
{
    private readonly string _baseScriptsPath;

    public GameEngineService(
        IOptionsSnapshot<GameConfiguration> gameConfig, IHostEnvironment environment)
    {
        _baseScriptsPath = Path.Combine(environment.ContentRootPath, gameConfig.Value.ScriptsDirectory);
    }

    public IList<string> GetAvailableScripts() =>
        Directory.Exists(_baseScriptsPath)
            ? Directory
                .GetFiles(_baseScriptsPath, "*.json")
                .Select(Path.GetFileNameWithoutExtension)
                .ToList()!
            : new List<string>();
}
