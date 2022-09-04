using Telegram.Bot.Types;

namespace StorytellerBot.Services
{
    public interface IMessageGeneratorFactory
    {
        IConversation? Create(Update update);
    }
}
