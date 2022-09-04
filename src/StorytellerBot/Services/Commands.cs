using Telegram.Bot.Types;

namespace StorytellerBot.Services;

internal static class Commands
{
    internal const string Start = "start";
    internal const string Restart = "restart";
    internal const string List = "list";

    internal static IDictionary<string, List<BotCommand>> GetAll()
    {
        return new Dictionary<string, List<BotCommand>>
        {
            {
                "es",
                new List<BotCommand>
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
                }
            },
        };
    }
}