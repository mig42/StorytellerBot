namespace StorytellerBot.Settings;

public class MessageSettings
{
    public int DelayBeforeSendMillis { get; set; }
    public TimeSpan DelayBeforeSend => TimeSpan.FromMilliseconds(DelayBeforeSendMillis);
}
