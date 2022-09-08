using StorytellerBot.Models.Game;
using Telegram.Bot.Types;

namespace StorytellerBot.Services;

public interface IConversation
{
    Task<IEnumerable<Response>> GetResponsesAsync(Update update);
}
