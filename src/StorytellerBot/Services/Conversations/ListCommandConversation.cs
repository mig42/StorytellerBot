using StorytellerBot.Data;
using StorytellerBot.Models.Game;
using Telegram.Bot.Types;

namespace StorytellerBot.Services.Conversations;

public class ListCommandConversation : IConversation
{
    private readonly AdventureRepository _repo;

    public ListCommandConversation(AdventureRepository repo)
    {
        _repo = repo;
    }

    async Task<IEnumerable<Response>> IConversation.GetResponsesAsync(Update update)
    {
        var adventures = await _repo.GetAllAdventuresAsync();
        return adventures.Select(a => new Response
        {
            ChatId = update.Message!.Chat.Id,
            Text = $"📜 *{a.Id}: _{a.Name}_*\n\n{a.Description}",
        });
    }
}
