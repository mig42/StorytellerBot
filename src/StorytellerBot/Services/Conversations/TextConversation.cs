using StorytellerBot.Data;
using Telegram.Bot.Types;

namespace StorytellerBot.Services.Conversations;

public class TextConversation : IConversation
{
    private readonly AdventureRepository _repo;
    private readonly IResponseSender _responseSender;
    private readonly IConversationFactory _conversationFactory;

    public TextConversation(
        AdventureRepository repo, IResponseSender responseSender, IConversationFactory conversationFactory)
    {
        _repo = repo;
        _responseSender = responseSender;
        _conversationFactory = conversationFactory;
    }

    async Task<IEnumerable<Message>> IConversation.SendResponsesAsync(Update update)
    {
        var user = await _repo.GetUserAsync(update.Message!.From!.Id);
        if (user?.CommandProgress == null)
        {
            return Array.Empty<Message>();
        }

        var conversation = _conversationFactory.CreateForCommand(user.CommandProgress.Command);
        return await conversation.SendResponsesAsync(update);
    }
}
