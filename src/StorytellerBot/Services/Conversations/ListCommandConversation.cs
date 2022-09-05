using Microsoft.EntityFrameworkCore;
using StorytellerBot.Data;
using StorytellerBot.Models;
using Telegram.Bot.Types;

namespace StorytellerBot.Services.Conversations;

public class ListCommandConversation : IConversation
{
    private readonly AdventureContext _context;
    private readonly IResponseSender _responseSender;

    public ListCommandConversation(AdventureContext context, IResponseSender responseSender)
    {
        _context = context;
        _responseSender = responseSender;
    }

    async Task<IEnumerable<Message>> IConversation.SendResponsesAsync(Update update)
    {
        var adventures = await _context.Adventures.AsNoTracking().ToListAsync();
        return await _responseSender.SendResponsesAsync(adventures.Select(a => new Response
        {
            ChatId = update.Message!.Chat.Id,
            Text = $"📜 *{a.Id}: _{a.Name}*_\n\n{a.Description}",
        }));
    }
}
