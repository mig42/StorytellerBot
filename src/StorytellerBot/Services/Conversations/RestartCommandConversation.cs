using Microsoft.EntityFrameworkCore;
using StorytellerBot.Data;
using StorytellerBot.Models;
using Telegram.Bot.Types;

using User = StorytellerBot.Models.User;

namespace StorytellerBot.Services.Conversations;

public class RestartCommandConversation : IRestartCommandConversation
{
    private readonly AdventureContext _context;
    private readonly IResponseSender _responseSender;

    public RestartCommandConversation(AdventureContext context, IResponseSender responseSender)
    {
        _context = context;
        _responseSender = responseSender;
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

        if (user.CurrentGame == null)
        {
            return Enumerable.Empty<Message>();
        }

        if (IsInvalidState(user.CommandProgress))
        {
            if (user.CommandProgress != null)
            {
                _context.CommandProgresses.Remove(user.CommandProgress);
            }

            var restartProgress = new CommandProgress
            {
                Command = Commands.Restart,
                Step = State.Confirm,
                UserId = user.Id,
            };
            user.CommandProgress = restartProgress;
            _context.CommandProgresses.Add(restartProgress);
            await _context.SaveChangesAsync();

            return await _responseSender.SendResponseAsync(new Response
            {
                ChatId = update.Message!.Chat.Id,
                Text = "Reiniciar la aventura borrará todos los datos de la partida actual. ¿Quieres continuar?",
            });
        }

        if (user.CommandProgress!.Step == State.Confirm)
        {
            user.CommandProgress = null;
            user.CurrentGame.StoryState = null;
            await _context.SaveChangesAsync();

            // TODO print first message
        }

        return Enumerable.Empty<Message>();
    }

    private bool IsInvalidState(CommandProgress? commandProgress)
    {
        if (commandProgress == null || commandProgress.Command != Commands.Restart)
        {
            return true;
        }
        return commandProgress.Step != State.Confirm;
    }

    private static class State
    {
        public const string Confirm = "confirm";
    }
}
