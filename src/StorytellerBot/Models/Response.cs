using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace StorytellerBot.Models;

public class Response
{
    public ChatId ChatId { get; init; } = null!;
    public string Text { get; init; } = string.Empty;
    public IReplyMarkup? ReplyMarkup { get; init; }
    public TimeSpan? Delay { get; init; }
    public bool IsEndOfAdventure { get; init; }
}
