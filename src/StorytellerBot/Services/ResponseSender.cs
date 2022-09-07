using System.Text.RegularExpressions;
using StorytellerBot.Models.Game;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace StorytellerBot.Services;

public class ResponseSender : IResponseSender
{
    private readonly ITelegramBotClient _botClient;
    private readonly IMediaLocator _mediaLocator;
    private readonly ILogger<ResponseSender> _logger;

    public ResponseSender(ITelegramBotClient botClient, IMediaLocator mediaLocator, ILogger<ResponseSender> logger)
    {
        _botClient = botClient;
        _mediaLocator = mediaLocator;
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

        var text = ProtectMessage(response.Text);
        var parseMode = ParseMode.MarkdownV2;
        if (_mediaLocator.IsExistingMedia(response.Image))
        {
            using Stream imageStream = _mediaLocator.OpenMedia(response.Image);
            return await _botClient.SendPhotoAsync(
                response.ChatId,
                new InputOnlineFile(imageStream, response.Image),
                text,
                parseMode,
                replyMarkup: response.ReplyMarkup);
        }
        return await _botClient.SendTextMessageAsync(
            response.ChatId,
            text,
            parseMode,
            replyMarkup: response.ReplyMarkup);
    }

    private static string ProtectMessage(string message)
    {
        return Regex.Replace(message, "[-.>]", match => $"\\{match.Value}");
    }
}
