using StorytellerBot.Data;
using StorytellerBot.Models;
using Telegram.Bot.Types;

namespace StorytellerBot.Services.Conversations;

public class ListCommandConversation : IConversation
{
    private readonly AdventureRepository _repo;
    private readonly IResponseSender _responseSender;

    public ListCommandConversation(AdventureRepository repo, IResponseSender responseSender)
    {
        _repo = repo;
        _responseSender = responseSender;
    }

    async Task<IEnumerable<Message>> IConversation.SendResponsesAsync(Update update)
    {
        var adventures = await _repo.GetAllAdventuresAsync();
        return await _responseSender.SendResponsesAsync(adventures.Select(a => new Response
        {
            ChatId = update.Message!.Chat.Id,
            Text = $"📜 *{a.Id}: _{a.Name}_*\n\n{a.Description}",
        }));
    }
}
