using Microsoft.Extensions.Options;
using StorytellerBot.Models;
using StorytellerBot.Settings;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace StorytellerBot.Services;

public class HandleUpdateService
{
    private readonly MessageSettings _messageSettings;
    private readonly GameEngineService _gameEngineService;
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<HandleUpdateService> _logger;

    public HandleUpdateService(
        IOptionsSnapshot<MessageSettings> messageSettings,
        GameEngineService gameEngineService,
        ITelegramBotClient botClient,
        ILogger<HandleUpdateService> logger)
    {
        _messageSettings = messageSettings.Value;
        _gameEngineService = gameEngineService;
        _botClient = botClient;
        _logger = logger;
    }

    public async Task EchoAsync(Update update)
    {
        var handler = update.Type switch
        {
            // UpdateType.Unknown:
            // UpdateType.ChannelPost:
            // UpdateType.EditedMessage:
            // UpdateType.EditedChannelPost:
            // UpdateType.ShippingQuery:
            // UpdateType.PreCheckoutQuery:
            // UpdateType.Poll:
            // UpdateType.InlineQuery:
            // UpdateType.ChosenInlineResult:
            UpdateType.Message            => BotOnMessageReceived(update.Message!),
            UpdateType.EditedMessage      => BotOnMessageReceived(update.EditedMessage!),
            UpdateType.CallbackQuery      => BotOnCallbackQueryReceived(update.CallbackQuery!),
            _                             => HandleUnsupportedUpdateAsync(update)
        };

        try
        {
            await handler;
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(exception, update);
        }
    }

    private async Task BotOnMessageReceived(Message message)
    {
        _logger.LogInformation("Receive message type: {MessageType}", message.Type);
        if (message.Type != MessageType.Text)
            return;

        var action = message.Text!.Split(' ')[0] switch
        {
            "/start"    => Reset(_botClient, message),
            _           => Text(_botClient, message)
        };

        var sentMessages = await action;
        _logger.LogInformation(
            "Sent message(s) with id(s): {SentMessageIds}",
            sentMessages.Select(m => m.MessageId));

        async Task<IEnumerable<Message>> Reset(ITelegramBotClient bot, Message msg)
        {
            IList<string> availableScripts = _gameEngineService.GetAvailableScripts();
            if (availableScripts.Any())
            {
                ReplyKeyboardMarkup replyKeyboardMarkup = new(
                    availableScripts.Select(name => new KeyboardButton[] { name }))
                    {
                        ResizeKeyboard = true,
                    };

                return new[]
                {
                    await SendMessage(
                    chatId: msg.Chat.Id,
                    text: "¿Qué historia quieres que te cuente?",
                    replyMarkup: replyKeyboardMarkup,
                    delayBeforeSend: TimeSpan.Zero)

                };
            }

            return new[]
            {
                await SendMessage(
                    chatId: msg.Chat.Id, text: "Aún no hay historias que contar :(", delayBeforeSend: TimeSpan.Zero)
            };
        }

        async Task<IEnumerable<Message>> Text(ITelegramBotClient bot, Message msg)
        {
            AdventureStep? adventureStep = await _gameEngineService.GetFirstMessage(msg.Text ?? string.Empty);

            var result = new List<Message>();
            if (adventureStep == null)
            {
                result.Add(await SendMessage(
                    chatId: msg.Chat.Id, text: "No te entiendo!", delayBeforeSend: TimeSpan.Zero));
                return result;
            }

            foreach (var paragraph in adventureStep.Paragraphs.SkipLast(1))
                result.Add(await SendMessage(msg.Chat.Id, paragraph));

            result.Add(await SendMessage(
                msg.Chat.Id,
                adventureStep.Paragraphs.Last(),
                BuildDecisionsInlineKeyboard(adventureStep)));

            return result;
        }

        static InlineKeyboardMarkup BuildDecisionsInlineKeyboard(AdventureStep adventureStep)
        {
            if (adventureStep.IsEnding)
                return InlineKeyboardMarkup.Empty();

            return new InlineKeyboardMarkup(adventureStep.Decisions.Select(decision => new[]
            {
                InlineKeyboardButton.WithCallbackData(decision.Text, decision.Path),
            }));
        }
    }

    // Process Inline Keyboard callback data
    private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
    {
        if (callbackQuery.Message == null)
        {
            _logger.LogWarning("Unable to handle callback #{CallbackId}, message was empty", callbackQuery.Id);
            return;
        }

        await _botClient.EditMessageReplyMarkupAsync(
            chatId: callbackQuery.Message.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            replyMarkup: InlineKeyboardMarkup.Empty());

        await SendMessage(callbackQuery.Message.Chat.Id, "Camino elegido: " + callbackQuery.Data);
    }

    private Task HandleUnsupportedUpdateAsync(Update update)
    {
        _logger.LogInformation(
            "Unsupported update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }

    private async Task HandleErrorAsync(Exception exception, Update update)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException => string.Join(
                Environment.NewLine,
                "Telegram API Error:",
                $"[{apiRequestException.ErrorCode}]",
                $"{apiRequestException.Message}"),
            _ => exception.ToString()
        };

        _logger.LogInformation("HandleError: {ErrorMessage}", errorMessage);
        if (update.Type == UpdateType.Message && update.Message != null)
        {
            await SendMessage(update.Message.Chat.Id, $"Error: {errorMessage}");
        }
    }

    async Task<Message> SendMessage(
        ChatId chatId,
        string text,
        IReplyMarkup? replyMarkup = null,
        TimeSpan? delayBeforeSend = null)
    {
        await Task.Delay(delayBeforeSend ?? _messageSettings.DelayBeforeSend);

        return await _botClient.SendTextMessageAsync(
            chatId: chatId,
            text: text,
            replyMarkup: replyMarkup ?? new ReplyKeyboardRemove());
    }
}
