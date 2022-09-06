using System.Text.RegularExpressions;
using StorytellerBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace StorytellerBot.Services;

public class ResponseSender : IResponseSender
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<ResponseSender> _logger;

    public ResponseSender(ITelegramBotClient botClient, ILogger<ResponseSender> logger)
    {
        _botClient = botClient;
        _logger = logger;
    }

    public async Task<IEnumerable<Message>> SendResponseAsync(Response response)
    {
        return new Message[] { await SendSingleResponseAsync(response) };
    }

    public async Task<IEnumerable<Message>> SendResponsesAsync(IEnumerable<Response> responses)
    {
        List<Message> result = new();
        foreach (var response in responses)
        {
            result.Add(await SendSingleResponseAsync(response));
        }
        return result;
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
        _logger.LogInformation("Sending response to chat {ChatId}: {Text}", response.ChatId, response.Text);
        return await _botClient.SendTextMessageAsync(
            chatId: response.ChatId,
            text: ProtectMessage(response.Text),
            parseMode: ParseMode.MarkdownV2,
            replyMarkup: response.ReplyMarkup);
    }

    private static string ProtectMessage(string message)
    {
        return Regex.Replace(message, "[-.>]", match => $"\\{match.Value}");
    }
}
