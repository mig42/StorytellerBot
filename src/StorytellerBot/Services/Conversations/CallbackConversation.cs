using StorytellerBot.Data;
using StorytellerBot.Models;
using Telegram.Bot.Types;

namespace StorytellerBot.Services.Conversations;

public class CallbackConversation : ICallbackConversation
{
    private readonly AdventureContext _context;
    private readonly IResponseSender _responseSender;
    private readonly ILogger<CallbackConversation> _logger;

    public CallbackConversation(
        AdventureContext context, IResponseSender responseSender, ILogger<CallbackConversation> logger)
    {
        _context = context;
        _responseSender = responseSender;
        _logger = logger;
    }

    async Task<IEnumerable<Message>> IConversation.SendResponsesAsync(Update update)
    {
        var callbackQuery = update.CallbackQuery!;

        if (callbackQuery.Message == null)
        {
            _logger.LogWarning("Unable to handle callback #{CallbackId}, message was empty", callbackQuery.Id);
            return Array.Empty<Message>();
        }

        await _responseSender.ClearInlineKeyboard(callbackQuery.Message.Chat, callbackQuery.Message.MessageId);

        // TODO load story
        // TODO save transition
        return await _responseSender.SendResponseAsync(new Response
        {
            ChatId = callbackQuery.Message.Chat,
            Text = $"Camino elegido: {callbackQuery.Data}",
        });
    }
}
