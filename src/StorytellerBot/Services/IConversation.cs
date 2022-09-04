using Telegram.Bot.Types;

namespace StorytellerBot.Services;

public interface IConversation
{
    Task<IEnumerable<Message>> SendResponsesAsync(Update update);
}

public interface ICallbackConversation : IConversation
{
}

public interface IListCommandConversation : IConversation
{
}

public interface IRestartCommandConversation : IConversation
{
}

public interface IStartCommandConversation : IConversation
{
}

public interface ITextConversation : IConversation
{
}
