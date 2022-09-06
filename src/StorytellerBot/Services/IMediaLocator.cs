namespace StorytellerBot.Services;

public interface IMediaLocator
{
    bool IsExistingMedia(string fileName);
    Stream OpenMedia(string fileName);
}
