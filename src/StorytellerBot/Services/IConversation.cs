using Telegram.Bot.Types;

namespace StorytellerBot.Services;

public interface IConversation
{
    Task<IEnumerable<Message>> SendResponsesAsync(Update update);
}
