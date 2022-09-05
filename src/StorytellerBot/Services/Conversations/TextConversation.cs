using StorytellerBot.Data;
using Telegram.Bot.Types;

namespace StorytellerBot.Services.Conversations;

public class TextConversation : IConversation
{
    private readonly AdventureContext _context;
    private IResponseSender _responseSender;
    private IConversationFactory _conversationFactory;

    public TextConversation(
        AdventureContext context, IResponseSender responseSender, IConversationFactory conversationFactory)
    {
        _context = context;
        _responseSender = responseSender;
        _conversationFactory = conversationFactory;
    }

    async Task<IEnumerable<Message>> IConversation.SendResponsesAsync(Update update)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == update.Message!.From!.Id);
        if (user?.CommandProgress == null)
        {
            return Array.Empty<Message>();
        }

        var conversation = _conversationFactory.CreateForCommand(user.CommandProgress.Command);
        return await conversation.SendResponsesAsync(update);
    }
}
