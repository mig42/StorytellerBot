using Microsoft.Extensions.Options;
using StorytellerBot.Settings;

namespace StorytellerBot.Services;

public class MediaLocator : IMediaLocator
{
    private readonly string _mediaPath;
    public MediaLocator(IOptions<GameConfiguration> config, IWebHostEnvironment environment)
    {
        _mediaPath = Path.Combine(environment.ContentRootPath, config.Value.MediaDirectory);
    }

    public bool IsExistingMedia(string fileName)
    {
        return File.Exists(Path.Combine(_mediaPath, fileName));
    }

    public Stream OpenMedia(string fileName)
    {
        return File.OpenRead(Path.Combine(_mediaPath, fileName));
    }
}
