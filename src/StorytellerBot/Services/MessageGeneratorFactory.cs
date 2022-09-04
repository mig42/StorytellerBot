using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using StorytellerBot.Services.Conversations;

namespace StorytellerBot.Services;

public class MessageGeneratorFactory : IMessageGeneratorFactory
{
    private readonly IServiceProvider _serviceProvider;

    public MessageGeneratorFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private static bool HasSender(Update update)
    {
        var message = update.Message ?? update.CallbackQuery?.Message;
        return message?.From != null;
    }

    IConversation? IMessageGeneratorFactory.Create(Update update)
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
            UpdateType.Message            => GetMessageGenerator(update.Message!),
            UpdateType.CallbackQuery      => _serviceProvider.GetService<CallbackConversation>(),
            _                             => null,
        };
    }

    private IConversation? GetMessageGenerator(Message message)
    {
        return message.Text switch
        {
            $"/{Commands.Start} " => _serviceProvider.GetService<StartCommandConversation>(),
            $"/{Commands.Restart} " => _serviceProvider.GetService<RestartCommandConversation>(),
            $"/{Commands.List} " => _serviceProvider.GetService<ListCommandConversation>(),
            _        => _serviceProvider.GetService<TextConversation>(),
        };
    }
}
