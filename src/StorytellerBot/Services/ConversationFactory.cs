using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using StorytellerBot.Services.Conversations;

namespace StorytellerBot.Services;

public class ConversationFactory : IConversationFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ConversationFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private static bool HasSender(Update update)
    {
        var message = update.Message ?? update.CallbackQuery?.Message;
        return message?.From != null;
    }

    public IConversation CreateForCommand(string command)
    {
        return GetConversationForCommand(command, false);
    }

    IConversation IConversationFactory.Create(Update update)
    {
        if (HasSender(update))
            return _serviceProvider.GetRequiredService<NoopConversation>();

        return update.Type switch
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
            UpdateType.Message            => GetConversationForCommand(GetCommandFromMessage(update.Message!), true),
            UpdateType.CallbackQuery      => _serviceProvider.GetRequiredService<CallbackConversation>(),
            _                             => _serviceProvider.GetRequiredService<NoopConversation>(),
        };
    }

    private static string GetCommandFromMessage(Message message)
    {
        var text = message.Text;
        if (text == null)
            return string.Empty;

        var command = text.Split(' ', 2)[0];
        return command.StartsWith('/') ? command[1..] : string.Empty;
    }

    private IConversation GetConversationForCommand(string command, bool allowTextConversation)
    {
        return command switch
        {
            Commands.Start   => _serviceProvider.GetRequiredService<StartCommandConversation>(),
            Commands.Restart => _serviceProvider.GetRequiredService<RestartCommandConversation>(),
            Commands.List    => _serviceProvider.GetRequiredService<ListCommandConversation>(),
            _                => allowTextConversation
                ? _serviceProvider.GetRequiredService<TextConversation>()
                : _serviceProvider.GetRequiredService<NoopConversation>(),
        };
    }
}
