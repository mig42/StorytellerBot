using StorytellerBot.Models.Game;
using Telegram.Bot.Types;

namespace StorytellerBot.Services.Conversations
{
    public class NoopConversation : IConversation
    {
        Task<IEnumerable<Response>> IConversation.GetResponsesAsync(Update update)
        {
            return Task.FromResult(Enumerable.Empty<Response>());
        }
    }
}
