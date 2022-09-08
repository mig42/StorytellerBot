using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace StorytellerBot.Models.Game;

public class Response
{
    public ChatId ChatId { get; init; } = null!;
    public string Text { get; init; } = string.Empty;
    public string Image { get; init; } = string.Empty;
    public IReplyMarkup ReplyMarkup { get; init; } = new ReplyKeyboardRemove();
    public bool IsEndOfAdventure { get; init; }
}
