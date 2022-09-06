using Telegram.Bot.Types;

namespace StorytellerBot.Services;

internal static class Commands
{
    internal const string Start = "start";
    internal const string Restart = "restart";
    internal const string List = "list";

    internal static bool IsCommand(string? message, string command)
    {
        if (message == null)
            return false;
        var parts = message.Split(' ', 2);
        return parts[0] == $"/{command}";
    }

    internal static List<BotCommand> GetAll()
    {
        return new List<BotCommand>
        {
            new BotCommand
            {
                Command = Start,
                Description = "Comienza una nueva aventura",
            },
            new BotCommand
            {
                Command = Restart,
                Description = "Reinicia la aventura actual",
            },
            new BotCommand
            {
                Command = List,
                Description = "Lista de aventuras",
            },
        };
    }
}
