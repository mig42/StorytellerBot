using StorytellerBot.Data;
using Telegram.Bot.Types;

namespace StorytellerBot.Services.Conversations;

public class CallbackConversation : IConversation
{
    private readonly AdventureContext _context;
    private readonly IResponseSender _responseSender;
    private readonly IAdventureWriter _adventureWriter;
    private readonly ILogger<CallbackConversation> _logger;

    public CallbackConversation(
        AdventureContext context,
        IResponseSender responseSender,
        IAdventureWriter adventureWriter,
        ILogger<CallbackConversation> logger)
    {
        _context = context;
        _responseSender = responseSender;
        _adventureWriter = adventureWriter;
        _logger = logger;
    }

    async Task<IEnumerable<Message>> IConversation.SendResponsesAsync(Update update)
    {
        var callbackQuery = update.CallbackQuery!;
        await _responseSender.ClearInlineKeyboard(callbackQuery.Message!.Chat, callbackQuery.Message.MessageId);

        var userId = callbackQuery.From!.Id;
        var user = _context.Users.FirstOrDefault(u => u.Id == userId);
        if (user?.CurrentGame == null)
        {
            _logger.LogWarning(
                "Unable to handle callback #{CallbackId}, unknown user #{UserId}", callbackQuery.Id, userId);
            return Enumerable.Empty<Message>();
        }

        if (callbackQuery.Message == null)
        {
            _logger.LogWarning("Unable to handle callback #{CallbackId}, message was empty", callbackQuery.Id);
            return Array.Empty<Message>();
        }

        if (!int.TryParse(callbackQuery.Data, out int choiceIndex))
        {
            _logger.LogWarning(
                "Unable to handle callback #{CallbackId}, choice index was not a number: {ChoiceIndex}",
                callbackQuery.Id, callbackQuery.Data);
            return Array.Empty<Message>();
        }

        user.CurrentGame = await _adventureWriter.AdvanceAdventureAsync(user.CurrentGame, choiceIndex);
        await _context.SaveChangesAsync();

        var responses = await _adventureWriter.GetCurrentStepMessagesAsync(callbackQuery.Message.Chat, user.CurrentGame);
        return await _responseSender.SendResponsesAsync(responses);
    }
}
