using System.Text;
using Microsoft.EntityFrameworkCore;
using StorytellerBot.Data;
using StorytellerBot.Models;
using Telegram.Bot.Types;

using User = StorytellerBot.Models.User;

namespace StorytellerBot.Services.Conversations;

public class StartCommandConversation : IConversation
{
    private readonly AdventureContext _context;
    private readonly IResponseSender _responseSender;
    private readonly IAdventureWriter _adventureWriter;

    public StartCommandConversation(
        AdventureContext context, IResponseSender responseSender, IAdventureWriter adventureWriter)
    {
        _context = context;
        _responseSender = responseSender;
        _adventureWriter = adventureWriter;
    }

    async Task<IEnumerable<Message>> IConversation.SendResponsesAsync(Update update)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == update.Message!.From!.Id);
        if (user == null)
        {
            user = new User { Id = update.Message!.From!.Id };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        if (IsInvalidState(user.CommandProgress))
        {
            if (user.CommandProgress != null)
            {
                _context.CommandProgresses.Remove(user.CommandProgress);
            }

            var startProgress = new CommandProgress
            {
                Command = Commands.Start,
                Step = State.Start,
                UserId = user.Id,
            };
            user.CommandProgress = startProgress;
            _context.CommandProgresses.Add(startProgress);
            await _context.SaveChangesAsync();

            return await _responseSender.SendResponseAsync(new Response
            {
                ChatId = update.Message!.Chat.Id,
                Text = await BuildStartMessageAsync(),
            });
        }

        if (user.CommandProgress!.Step == State.Start)
        {
            int? adventureId = ParseAdventureId(update.Message!);
            if (adventureId == null)
            {
                return await _responseSender.SendResponseAsync(new Response
                {
                    ChatId = update.Message!.Chat.Id,
                    Text = "No entiendo ese número. Por favor, escribe el número de la aventura que quieres jugar.",
                });
            }

            if (!(await _context.Adventures.AsNoTracking().AnyAsync(a => a.Id == adventureId)))
            {
                return await _responseSender.SendResponseAsync(new Response
                {
                    ChatId = update.Message!.Chat.Id,
                    Text = "No existe ninguna aventura con ese número. Por favor, escribe el número de la aventura que quieres jugar.",
                });
            }

            if (user.SavedGames.Any(a => a.Id == adventureId))
            {
                user.CommandProgress.Argument = adventureId.ToString();
                user.CommandProgress.Step = State.Confirm;
                await _context.SaveChangesAsync();

                return await _responseSender.SendResponseAsync(new Response
                {
                    ChatId = update.Message!.Chat.Id,
                    Text = "Tienes una aventura guardada con ese número, se borrará su progreso. ¿Quieres continuar?",
                });
            }

            user.CommandProgress = null;
            user.CurrentGame = new SavedStatus
            {
                AdventureId = adventureId.Value,
                UserId = user.Id,
                LastUpdated = DateTime.UtcNow,
            };
            await _context.SaveChangesAsync();
        }

        if (user.CommandProgress!.Step == State.Confirm)
        {
            var text = update.Message!.Text!.ToLowerInvariant();
            if (text == "si" || text == "sí")
            {
                var savedGame = user.SavedGames.First(s => s.AdventureId == int.Parse(user.CommandProgress.Argument!));
                user.SavedGames.Remove(savedGame);
                _context.SavedStatuses.Remove(savedGame);

                user.CurrentGame = new SavedStatus
                {
                    AdventureId = savedGame.AdventureId,
                    UserId = user.Id,
                    LastUpdated = DateTime.UtcNow,
                };
                await _context.SaveChangesAsync();
            }
            else
            {
                user.CommandProgress = null;
                await _context.SaveChangesAsync();
                return Enumerable.Empty<Message>();
            }
        }
        return await _responseSender.SendResponsesAsync(
            await _adventureWriter.GetCurrentStepMessagesAsync(update.Message!.Chat, user.CurrentGame));

    }

    private static bool IsInvalidState(CommandProgress? command)
    {
        if (command is null || command.Command != Commands.Start)
        {
            return true;
        }

        return command.Step != State.Start && command.Step != State.Confirm;
    }

    private static int? ParseAdventureId(Message message)
    {
        return int.TryParse(message?.Text, out int adventureId) ? adventureId : null;
    }

    private async Task<string> BuildStartMessageAsync()
    {
        var adventures = await _context.Adventures.AsNoTracking().ToListAsync();

        StringBuilder sb = new();
        sb.AppendLine("*Elige una aventura:*");
        sb.AppendLine();

        foreach (var adventure in adventures)
        {
            sb.AppendLine($"{adventure.Id}. {adventure.Name}");
        }

        sb.AppendLine();
        sb.AppendLine("Envíame el número de la aventura que quieras jugar.");

        return sb.ToString();
    }

    static class State
    {
        public const string Start = "start";
        public const string Confirm = "confirm";
    }
}
