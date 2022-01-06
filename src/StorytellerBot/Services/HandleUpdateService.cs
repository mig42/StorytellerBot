using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace StorytellerBot.Services;

public class HandleUpdateService
{
    private readonly GameEngineService _gameEngineService;
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<HandleUpdateService> _logger;

    public HandleUpdateService(
        GameEngineService gameEngineService,
        ITelegramBotClient botClient,
        ILogger<HandleUpdateService> logger)
    {
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
            await HandleErrorAsync(exception);
        }
    }

    private async Task BotOnMessageReceived(Message message)
    {
        _logger.LogInformation("Receive message type: {messageType}", message.Type);
        if (message.Type != MessageType.Text)
            return;

        var action = message.Text!.Split(' ')[0] switch
        {
            "/start"    => Reset(_botClient, message),
            _           => Fallback(_botClient, message)
        };
        Message sentMessage = await action;
        _logger.LogInformation("The message was sent with id: {sentMessageId}",sentMessage.MessageId);

        async Task<Message> Reset(ITelegramBotClient bot, Message message)
        {
            IList<string> availableScripts = _gameEngineService.GetAvailableScripts();
            if (availableScripts.Any())
            {
                ReplyKeyboardMarkup replyKeyboardMarkup = new(
                    availableScripts.Select(name => new KeyboardButton[] { name }))
                    {
                        ResizeKeyboard = true,
                    };

                return await bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "¿Qué historia quieres que te cuente?",
                    replyMarkup: replyKeyboardMarkup);
            }

            return await bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Aún no hay historias que contar :(",
                replyMarkup: new ReplyKeyboardRemove());
        }

        static async Task<Message> Fallback(ITelegramBotClient bot, Message message)
        {
            return await bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "No te entiendo!",
                replyMarkup: new ReplyKeyboardRemove());
        }
    }

    // Process Inline Keyboard callback data
    private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
    {
        await Task.Yield();
        // Important: manage callback
        // call_botClient.AnswerCallbackQueryAsync and _botClient.SendTextMessageAsync
    }

    private Task HandleUnsupportedUpdateAsync(Update update)
    {
        _logger.LogInformation("Unsupported update type: {updateType}", update.Type);
        return Task.CompletedTask;
    }

    private Task HandleErrorAsync(Exception exception)
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

        _logger.LogInformation("HandleError: {errorMessage}", errorMessage);
        return Task.CompletedTask;
    }
}
