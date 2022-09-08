using StorytellerBot.Data;
using StorytellerBot.Models.Game;
using Telegram.Bot.Types;

namespace StorytellerBot.Services.Conversations;

public class TextConversation : IConversation
{
    private readonly AdventureRepository _repo;
    private readonly IConversationFactory _conversationFactory;

    public TextConversation(AdventureRepository repo, IConversationFactory conversationFactory)
    {
        _repo = repo;
        _conversationFactory = conversationFactory;
    }

    async Task<IEnumerable<Response>> IConversation.GetResponsesAsync(Update update)
    {
        var commandProgress = await _repo.GetCommandProgressForUserAsync(update.Message!.From!.Id);
        if (commandProgress == null)
        {
            return Enumerable.Empty<Response>();
        }

        var conversation = _conversationFactory.CreateForCommand(commandProgress.Command);
        return await conversation.GetResponsesAsync(update);
    }
}
