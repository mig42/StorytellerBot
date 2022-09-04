using StorytellerBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace StorytellerBot.Services;

public class ResponseSender : IResponseSender
{
    private readonly ITelegramBotClient _botClient;

    public ResponseSender(ITelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    public async Task<IEnumerable<Message>> SendResponseAsync(Response response)
    {
        return new Message[] { await SendSingleResponseAsync(response) };
    }

    public async Task<IEnumerable<Message>> SendResponsesAsync(IEnumerable<Response> responses)
    {
        return await Task.WhenAll(responses.Select(r => SendSingleResponseAsync(r)));
    }

    public async Task ClearInlineKeyboard(ChatId chatId, int messageId)
    {
        await _botClient.EditMessageReplyMarkupAsync(
            chatId, messageId, replyMarkup: InlineKeyboardMarkup.Empty());
    }

    private async Task<Message> SendSingleResponseAsync(Response response)
    {
        if (response.Delay != null)
        {
            await Task.Delay(response.Delay.Value);
        }
        return await _botClient.SendTextMessageAsync(
            chatId: response.ChatId,
            text: response.Text,
            replyMarkup: response.ReplyMarkup);
    }
}
