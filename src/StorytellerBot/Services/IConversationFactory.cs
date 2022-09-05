using Telegram.Bot.Types;

namespace StorytellerBot.Services
{
    public interface IConversationFactory
    {
        IConversation Create(Update update);
        IConversation CreateForCommand(string command);
    }
}
