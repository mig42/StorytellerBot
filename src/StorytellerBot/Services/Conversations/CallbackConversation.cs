using StorytellerBot.Data;
using StorytellerBot.Models.Game;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace StorytellerBot.Services.Conversations;

public class CallbackConversation : IConversation
{
    private readonly AdventureRepository _repo;
    private readonly ITelegramBotClient _botClient;
    private readonly IAdventureWriter _adventureWriter;
    private readonly ILogger<CallbackConversation> _logger;

    public CallbackConversation(
        AdventureRepository repo,
        ITelegramBotClient botClient,
        IAdventureWriter adventureWriter,
        ILogger<CallbackConversation> logger)
    {
        _repo = repo;
        _botClient = botClient;
        _adventureWriter = adventureWriter;
        _logger = logger;
    }

    async Task<IEnumerable<Response>> IConversation.GetResponsesAsync(Update update)
    {
        var callbackQuery = update.CallbackQuery!;

        await _botClient.EditMessageReplyMarkupAsync(
            callbackQuery.Message!.Chat, callbackQuery.Message.MessageId, replyMarkup: InlineKeyboardMarkup.Empty());

        var userId = callbackQuery.From!.Id;
        var currentGame = await _repo.GetCurrentGameForUserAsync(userId);
        if (currentGame == null)
        {
            _logger.LogWarning(
                "Unable to handle callback #{CallbackId}, unknown user #{UserId}", callbackQuery.Id, userId);
            return Enumerable.Empty<Response>();
        }

        if (callbackQuery.Message == null)
        {
            _logger.LogWarning("Unable to handle callback #{CallbackId}, message was empty", callbackQuery.Id);
            return Array.Empty<Response>();
        }

        if (!int.TryParse(callbackQuery.Data, out int choiceIndex))
        {
            _logger.LogWarning(
                "Unable to handle callback #{CallbackId}, choice index was not a number: {ChoiceIndex}",
                callbackQuery.Id, callbackQuery.Data);
            return Enumerable.Empty<Response>();
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
        return responses;
    }
}
