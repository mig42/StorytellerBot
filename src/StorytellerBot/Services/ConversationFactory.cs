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

    public IConversation? CreateForCommand(string command)
    {
        return command switch
        {
            Commands.Start   => _serviceProvider.GetRequiredService<StartCommandConversation>(),
            Commands.Restart => _serviceProvider.GetRequiredService<RestartCommandConversation>(),
            Commands.List    => _serviceProvider.GetRequiredService<ListCommandConversation>(),
            _                => _serviceProvider.GetRequiredService<NoopConversation>(),
        };
    }

    IConversation? IConversationFactory.Create(Update update)
    {
        if (HasSender(update))
            return null;

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
            UpdateType.Message            => GetConversation(update.Message!),
            UpdateType.CallbackQuery      => _serviceProvider.GetService<CallbackConversation>(),
            _                             => _serviceProvider.GetService<NoopConversation>(),
        };
    }

    private IConversation? GetConversation(Message message)
    {
        return message.Text switch
        {
            $"/{Commands.Start} "   => _serviceProvider.GetService<StartCommandConversation>(),
            $"/{Commands.Restart} " => _serviceProvider.GetService<RestartCommandConversation>(),
            $"/{Commands.List} "    => _serviceProvider.GetService<ListCommandConversation>(),
            _                       => _serviceProvider.GetService<TextConversation>(),
        };
    }
}
