using Telegram.Bot.Types;

namespace StorytellerBot.Services.Conversations
{
    public class NoopConversation : IConversation
    {
        Task<IEnumerable<Message>> IConversation.SendResponsesAsync(Update update)
        {
            return Task.FromResult(Enumerable.Empty<Message>());
        }
    }
}
