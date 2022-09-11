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

    public async Task<List<Message>> SendResponsesAsync(IEnumerable<Response> responses)
    {
        List<Message> result = new();
        foreach (var response in responses)
        {
            Message? message;
            try
            {
                message = await SendSingleResponseAsync(response);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error sending response");
                message = await SendErrorNotificationAsync(response.ChatId, e.Message);
            }

            if (message != null)
            {
                result.Add(message);
            }
        }
        return result;
    }

    private async Task<Message> SendSingleResponseAsync(Response response)
    {
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
        return Regex.Replace(message, "[-.>!()]", match => $"\\{match.Value}");
    }

    private async Task<Message?> SendErrorNotificationAsync(ChatId chatId, string message)
    {
        try
        {
            return await _botClient.SendTextMessageAsync(
                chatId,
                $"Hubo un error procesando un mensaje. Notifícaselo al autor. Puedes usar el comando /{Commands.Restart} para comenzar de nuevo.\n\n ```{message}```",
                replyMarkup: new ReplyKeyboardRemove());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error sending error notification");
            return null;
        }
    }
}
