using StorytellerBot.Models.Game;
using Telegram.Bot.Types;

namespace StorytellerBot.Services;

public interface IResponseSender
{
    Task<List<Message>> SendResponsesAsync(IEnumerable<Response> responses);
}
