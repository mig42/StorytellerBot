using StorytellerBot.Data;
using Telegram.Bot.Types;

namespace StorytellerBot.Services.Conversations;

public class CallbackConversation : IConversation
{
    private readonly AdventureRepository _repo;
    private readonly IResponseSender _responseSender;
    private readonly IAdventureWriter _adventureWriter;
    private readonly ILogger<CallbackConversation> _logger;

    public CallbackConversation(
        AdventureRepository repo,
        IResponseSender responseSender,
        IAdventureWriter adventureWriter,
        ILogger<CallbackConversation> logger)
    {
        _repo = repo;
        _responseSender = responseSender;
        _adventureWriter = adventureWriter;
        _logger = logger;
    }

    async Task<IEnumerable<Message>> IConversation.SendResponsesAsync(Update update)
    {
        var callbackQuery = update.CallbackQuery!;
        await _responseSender.ClearInlineKeyboard(callbackQuery.Message!.Chat, callbackQuery.Message.MessageId);

        var userId = callbackQuery.From!.Id;
        var currentGame = await _repo.GetCurrentGameForUserAsync(userId);
        if (currentGame == null)
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

        var newStoryState = await _adventureWriter.AdvanceAdventureAsync(
            currentGame.SavedStatus, choiceIndex);
        await _repo.UpdateSavedStatusAsync(currentGame.SavedStatus, newStoryState, DateTime.UtcNow);

        var responses = await _adventureWriter.GetCurrentStepMessagesAsync(
            callbackQuery.Message.Chat, currentGame.SavedStatus);
        if (responses.Any(r => r.IsEndOfAdventure))
        {
            await _repo.DeleteSavedStatusAsync(currentGame.SavedStatus);
        }
        return await _responseSender.SendResponsesAsync(responses);
    }
}
