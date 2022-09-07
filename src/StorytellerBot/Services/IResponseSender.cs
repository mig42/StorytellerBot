using StorytellerBot.Models.Game;
using Telegram.Bot.Types;

namespace StorytellerBot.Services;

public interface IResponseSender
{
    Task<IEnumerable<Message>> SendResponseAsync(Response response);
    Task<IEnumerable<Message>> SendResponsesAsync(IEnumerable<Response> responses);
    Task ClearInlineKeyboard(ChatId chatId, int messageId);
}
